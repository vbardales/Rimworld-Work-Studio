import { access, readFile } from 'node:fs/promises';
import { join, resolve } from 'node:path';
import steam from 'semantic-release-steam';
import { compileReadme } from 'semantic-release-steam/lib/readme.mjs';
import { renderSteamBBCode } from 'semantic-release-steam/lib/description.mjs';

const publishing = (context) => context.env.STEAM_PUBLISH === 'true';

async function checkMod(mod, pluginConfig, cwd, logger) {
  const modPath = resolve(cwd, mod.path);
  try {
    await access(join(modPath, 'README.template.md'));
  } catch {
    throw new Error(`${mod.path}/README.template.md is missing: the Steam plugin would fail after the tag and GitHub release were created`);
  }
  let ignore = '';
  try {
    ignore = await readFile(join(modPath, '.steamignore'), 'utf8');
  } catch {
    // reported below with the other missing entries
  }
  const lines = ignore.split(/\r?\n/).map((line) => line.trim());
  const missing = ['README.template.md', 'README.md'].filter((name) => !lines.includes(`/${name}`) && !lines.includes(name));
  if (missing.length > 0) {
    throw new Error(`${mod.path}/.steamignore must list /${missing.join(' and /')} (anchored to the mod root), or they ship to players`);
  }
  const description = renderSteamBBCode(await compileReadme({
    modPath,
    header: pluginConfig.descriptionHeader ?? '',
    footer: pluginConfig.descriptionFooter ?? '',
    assetDirNameTransform: pluginConfig.assetDirNameTransform,
  }));
  logger.log(`Steam description for ${mod.name}: ${description.length} characters of BBCode`);
}

export async function verifyConditions(pluginConfig, context) {
  for (const mod of pluginConfig.mods) {
    await checkMod(mod, pluginConfig, context.cwd ?? process.cwd(), context.logger);
  }
  if (publishing(context)) await steam.verifyConditions(pluginConfig, context);
}

export async function publish(pluginConfig, context) {
  if (!publishing(context)) return undefined;
  const notes = context.nextRelease.notes ? renderSteamBBCode(context.nextRelease.notes) : context.nextRelease.version;
  return steam.publish(pluginConfig, { ...context, nextRelease: { ...context.nextRelease, notes } });
}
