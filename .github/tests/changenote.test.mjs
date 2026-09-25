import { test } from 'node:test';
import assert from 'node:assert/strict';
import { changenoteFor } from '../scripts/changenote.mjs';

const publication = [
  '# Publication',
  '',
  '### 1.0.2 — pending',
  '',
  'Some text.',
  '',
  '```',
  '[h3]1.0.2[/h3]',
  '',
  '[list]',
  '[*]one',
  '[/list]',
  '```',
  '',
  '### 1.0.0 - 2026-09-21',
  '',
  '```',
  '[h3]1.0.0[/h3]',
  '```',
].join('\n');

test('returns the fenced block under the matching version heading', () => {
  assert.equal(changenoteFor(publication, '1.0.2'), '[h3]1.0.2[/h3]\n\n[list]\n[*]one\n[/list]');
});

test('a version whose heading is followed by a date is found too', () => {
  assert.equal(changenoteFor(publication, '1.0.0'), '[h3]1.0.0[/h3]');
});

test('handles CRLF line endings', () => {
  assert.equal(changenoteFor(publication.replaceAll('\n', '\r\n'), '1.0.0'), '[h3]1.0.0[/h3]');
});

test('a missing version fails', () => {
  assert.throws(() => changenoteFor(publication, '9.9.9'), /no "### 9\.9\.9" section/);
});

test('a version is not matched as a prefix of a longer one', () => {
  assert.throws(() => changenoteFor('### 1.0.20\n```\nx\n```', '1.0.2'), /no "### 1\.0\.2" section/);
});

test('does not borrow the fenced block of the next section', () => {
  assert.throws(() => changenoteFor('### 1.0.2\nno fence\n### 1.0.1\n```\nx\n```', '1.0.2'), /no fenced change note/);
});

test('an unclosed or empty block fails', () => {
  assert.throws(() => changenoteFor('### 1.0.2\n```\nx', '1.0.2'), /not closed/);
  assert.throws(() => changenoteFor('### 1.0.2\n```\n\n```', '1.0.2'), /is empty/);
});
