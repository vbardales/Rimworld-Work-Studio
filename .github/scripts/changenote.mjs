// The text sent to Steam (change note, description) is the fenced block under a heading of a
// markdown file, so there is no second copy that could drift from it.
export function fencedBlockUnder(text, headingRegex, { label, what }) {
  const lines = text.split(/\r?\n/);
  const start = lines.findIndex((line) => headingRegex.test(line));
  if (start === -1) throw new Error(`no ${label} section found`);
  const open = lines.findIndex((line, i) => i > start && /^```/.test(line));
  const next = lines.findIndex((line, i) => i > start && /^#{1,6} /.test(line));
  if (open === -1 || (next !== -1 && open > next)) throw new Error(`the ${label} section has no fenced ${what}`);
  const close = lines.findIndex((line, i) => i > open && /^```\s*$/.test(line));
  if (close === -1) throw new Error(`the ${what} of ${label} is not closed`);
  const block = lines.slice(open + 1, close).join('\n').trim();
  if (!block) throw new Error(`the ${what} of ${label} is empty`);
  return block;
}

export function changenoteFor(publication, version) {
  const heading = new RegExp(`^### +${version.replaceAll('.', '\\.')}(\\s|$)`);
  return fencedBlockUnder(publication, heading, { label: `"### ${version}"`, what: 'change note' });
}
