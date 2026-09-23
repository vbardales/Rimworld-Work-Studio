const plugins = [
  '@semantic-release/commit-analyzer',
  '@semantic-release/release-notes-generator',
  '@semantic-release/github',
  ['./release-steam-plugin.mjs', { appId: '294100', branchTargets: { "main": 'stable' }, mods: [{ name: "Work Studio", path: 'Mod', workshopIds: { stable: "3792836684" } }] }],
];
export default { branches: ["main"], plugins };
