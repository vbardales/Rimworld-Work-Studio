// Skill and work type icon drawings, the SOURCE the 35 PNGs under
// Mod/Textures/WorkStudio/ were rasterised from.
//
// Taken 2026-09-18 from the SkillIcons mod, commit 80d3446, which handed this feature to Work
// Studio: its _tools/gen.js lines 1-261 (palette and primitives) and 1144-1426 (the two icon
// sets). The passion drawings that share those primitives stayed there - they are that mod's,
// and it keeps them.
//
// NOT WIRED TO A BUILD HERE. Work Studio has no _tools chain: this file is kept so the drawings
// stay changeable rather than becoming 35 PNGs nobody can redraw. To regenerate, run it with
// node against the same layout SkillIcons used (dirs.sil for the silhouette sheet), then
// rasterise the SVGs through headless Chrome as its _tools/build.sh does.
//
// THE RULE THAT COMES WITH THEM, and it is not optional: this set is monochrome, shape alone.
// Judge every drawing on the SILHOUETTE SHEET AT 20 px, never at 64 px - all four failures
// SkillIcons records (rifle to a smear, trowel to a down arrow, pickaxe to an umbrella, flame to
// a water drop) look perfectly fine at 64. See BACKLOG.md.

// Generates the 34 distinct icons, as SVG, in Oracle's graphic language.
// node gen.js  ->  ../svg/*.svg   and   ./sil/*.svg (black silhouettes, readability test)
const fs = require('fs');

// ---------------------------------------------------------------- palette
const P = {
  red:'#CC362F', grey:'#939393',
  greenP:'#387F36', orangeP:'#F79839', leaf:'#5AA83F',
  ice:'#6FBBDD', frost:'#D8F0FA', amber:'#E8A33D', purple:'#A96FC4',
  lav:'#B39DDB', pink:'#E07A9F', steel:'#8FA0B0', toxic:'#7CB342',
  gold:'#E0B54A', cream:'#D9C9A3', slate:'#5F7C8A',
  crimson:'#A3202B', greyPurple:'#8A7F94',
  iceP:'#6FBBDD',   // same blue, but used as the main shape (frozen)
  amberP:'#E8A33D', // drunken's glass: amber carries the shape
  silver:'#B8C4CE', // vengeful's blade, lighter than dormant steel
  flash:'#FFF6D0',  // pain-driven's flash of pain, two frames out of twenty-four
  neon:'#4FE8D8',   // transhumanist's signal trail
  blanc:'#F4F8FA',  // its bright head
  trace:'#14595C',  // DARK trace: in light silver, the neon dots stopped
                    // standing out once the chip turned teal.
                    // A bright signal demands a trace darker than itself.
                    // A DELIBERATELY DEEP colour: as an
                    // accessory they would take the same grey as the machine
                    // half and disappear in the Work tab
  ghost:'#4A4A4A',  // the "None" passion: present, but ghostly

  // ------------------------------------------------------------------------
  // ONE HUE PER PASSION, spread across the whole colour wheel.
  //
  // This is the single most important fix this set has had: 21 icons out of
  // 40 had a red mass and a small coloured accessory. At 24 px the accessory
  // disappears, and all that was left was a row of near-identical red blobs.
  // The hue must carry the identity, not the accessory.
  tDedicated:'#B4462F',    // brick red
  tObsessive:'#C2185B',    // magenta
  tSynergistic:'#1FA5A0',  // turquoise
  tFrozen:'#9FDCF2',       // very light glacier blue
  tNight:'#2E4A8C',        // night blue
  tDrunken:'#E0A02E',      // amber
  tYouth:'#F2726B',        // coral pink
  tVengeful:'#8E1F22',     // dark red
  tForbidden:'#4A4258',    // very dark violet
  tNomadic:'#D9B54A',      // ochre
  tSanguine:'#B3121C',     // blood red
  tToxic:'#8FD130',        // acid green
  tPain:'#FF7A1A',         // electric orange
  tBlind:'#E8E0CC',        // ivory
  tCompetitive:'#C9962C',  // deep gold: the body must be DARKER than
                           // the crown, otherwise the two blend together
  orClair:'#F7DE7A',       // the crown's light gold
  tDunce:'#8A6B54',        // taupe brown
  tIdeological:'#F08A70',  // salmon pink, close to the Ideology symbol
  tIntimate:'#E86BA0',     // pink
  tLikeMinded:'#6FC7D6',   // soft cyan
  tRainy:'#4C86C4',        // rain blue
  tStoned:'#7A8B3A',       // olive green
  tTranshuman:'#1F8A86',   // deep teal. The table's bright cyan
                           // would make the bright cyan dots running
                           // across the chip invisible: same family, but dark
                           // enough for them to stand out.
  tTraumatic:'#6B7F8C',    // desaturated blue-grey
  tNudist:'#F0A878',       // peach
};

// Two greys for the Work tab grid, two steels for the dormant states. In
// both cases: the main shape takes the light value, the accessory the dark
// one - flattening to a single value destroys the crown, the padlock and
// the snowflake.
const PRIMARY = ['red','purple','lav','toxic','slate','greyPurple','pink','iceP',
                 'amberP','greenP','orangeP','trace',
                 'tDedicated','tObsessive','tSynergistic','tFrozen','tNight','tDrunken',
                 'tYouth','tVengeful','tForbidden','tNomadic','tSanguine','tToxic',
                 'tPain','tBlind','tCompetitive','tDunce','tIdeological','tIntimate',
                 'tLikeMinded','tRainy','tStoned','tTranshuman','tTraumatic','tNudist'];
const GREY   = k => PRIMARY.includes(k) ? '#939393' : '#6B6B6B';

// Two independent axes, because a single one cannot carry both pieces of
// information without identity and speed treading on each other:
//
//   HUE says which passion       - toxic green, night blue, amber
//   SATURATION says how fast you learn
//   STEEL says the bonus is asleep (the untriggered state of a triggered passion)
//
// NEUTRAL, no longer the blue-tinted steel #8FA0B0: a blue-grey belongs to
// the same hue family as frozen's, night's or blind's blue, and so read as
// a thematic colour instead of a state.
const DORMANT = k => PRIMARY.includes(k) ? '#8C8C8C' : '#606060';

// blends two colours. Takes colours ALREADY resolved by the palette, so
// that the grey variant blends between two greys and stays grey.
const melange = (h1, h2, t) => {
  const p = h => [1, 3, 5].map(i => parseInt(h.substr(i, 2), 16));
  const [a, b] = [p(h1), p(h2)];
  return '#' + a.map((v, i) =>
    Math.round(v + (b[i] - v) * t).toString(16).padStart(2, '0')).join('');
};

// desaturates toward luminance: the hue survives, the intensity drops
const fade = (hex, f) => {
  const n = parseInt(hex.slice(1), 16);
  const r = n >> 16, g = (n >> 8) & 255, b = n & 255;
  const l = 0.299 * r + 0.587 * g + 0.114 * b;
  const m = v => Math.round(v + (l - v) * f).toString(16).padStart(2, '0');
  return '#' + m(r) + m(g) + m(b);
};

// --------- speed carried by BOTH saturation and lightness, continuously
//
// The four previous rungs had three measurable flaws: a cliff between 1.99
// and 2 where the real gap is negligible; 1.25, 1.5 and 1.75 all rendered
// identically; and, worse, light hues (gold) looking stronger than dark
// ones (crimson) at equal speed, since only saturation was touched.
//
// Hence the SHARED lightness target: at equal speed, icons converge toward
// the same clarity regardless of their hue. That is what keeps the
// progression readable in a grid where passions of different hues sit next
// to each other.
const versHsl = hex => {
  const n = parseInt(hex.slice(1), 16);
  const r = ((n >> 16) & 255) / 255, g = ((n >> 8) & 255) / 255, b = (n & 255) / 255;
  const max = Math.max(r, g, b), min = Math.min(r, g, b), l = (max + min) / 2;
  if (max === min) return [0, 0, l];
  const d = max - min;
  const s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
  const h = max === r ? ((g - b) / d + (g < b ? 6 : 0)) / 6
          : max === g ? ((b - r) / d + 2) / 6
                      : ((r - g) / d + 4) / 6;
  return [h, s, l];
};
const versHex = ([h, s, l]) => {
  const f = n => {
    const k = (n + h * 12) % 12, a = s * Math.min(l, 1 - l);
    const v = l - a * Math.max(-1, Math.min(k - 3, 9 - k, 1));
    return Math.round(Math.max(0, Math.min(1, v)) * 255).toString(16).padStart(2, '0');
  };
  return '#' + f(0) + f(8) + f(4);
};
// Deliberate highlights escape the ramp: their role is to be vivid, not to
// say a speed.
const REHAUTS = ['flash', 'blanc', 'frost'];
// DELIBERATELY SHALLOW amplitude: speed is only a suggestion. The hierarchy
// is hue = the passion's identity, lightness = active or dormant state,
// shape = confirmation. A wide ramp made slow hues so dull they stopped
// being identifiable - speed was eating identity.
const bande = lrf => {
  const t = Math.max(0, Math.min(1, (lrf || 0) / 3));
  return k => {
    if (REHAUTS.includes(k)) return P[k];
    const [h, s, l] = versHsl(P[k]);
    return versHex([h, s * (0.78 + 0.22 * t), l + ((0.42 + 0.12 * t) - l) * 0.25]);
  };
};

// learnRateFactor per file, read off the defs (maximum when several defs
// share the same texture, e.g. blind elevated/sublime)
const LRF = {
  AS_DrunkenPassion: 2.5, AS_NightPassion: 2.5, AS_NomadicPassion: 2.25,
  AS_PainDrivenPassion: 2, AS_SanguinePassion: 2, AS_StonedPassion: 1.8,
  AS_ToxicPassion: 2, AS_VengefulPassion: 3, AS_CompetitivePassion: 2.5,
  AS_RainyDayPassion: 2, AS_BlindPassion_Active: 2,
  AS_IdeologicalPassion_Active: 1.75, AS_IntimatePassion_Active: 1.75,
  AS_NudistPassion_Active: 2, AS_TranshumanistPassion_Active: 1.5,
  AS_DedicatedPassion: 1.25, AS_ForbiddenPassion: 1.75, AS_ObsessivePassion: 1.25,
  AS_YouthPassion: 3, AS_SynergisticPassion: 1, AS_DuncePassion: 0,
  AS_LikeMindedPassion: 1, AS_TraumaticPassion: 1, AS_FrozenPassion: 0,
  AS_MoodyPassion: 1, AS_MoodyPassion_Apathy: 0.1, AS_MoodyPassion_NoPassion: 0.35,
  AS_MoodyPassion_Major: 1.5, AS_MoodyPassion_Greater: 2,
  AS_PsychicPassion: 1, AS_PsychicPassion_Minor: 0.5, AS_PsychicPassion_Nullified: 0.1,
  AS_PsychicPassion_Major: 1.5, AS_PsychicPassion_Critical: 2,
};

// ---------------------------------------------- the two canonical shapes
const HEART = 'M32,52C32,52 10,36.5 10,23C10,16.6 14.9,12 20.6,12'
            + 'C25.6,12 29.7,15 32,19C34.3,15 38.4,12 43.4,12'
            + 'C49.1,12 54,16.6 54,23C54,36.5 32,52 32,52Z';
const FLAME = 'M32,11C40,22 47,28 47,36C47,44.5 40.5,51 32,51'
            + 'C23.5,51 17,44.5 17,36C17,28 24,22 32,11Z';

// maps every absolute coordinate pair (M/C/Z path commands only)
function xf(d, s, dx = 0, dy = 0, cx = 32, cy = 32) {
  const nums = d.match(/-?\d+(?:\.\d+)?/g).map(Number);
  let i = 0;
  return d.replace(/-?\d+(?:\.\d+)?/g, () => {
    const v = nums[i], isX = i % 2 === 0; i++;
    const c = isX ? cx : cy, o = isX ? dx : dy;
    return +((v - c) * s + c + o).toFixed(2);
  });
}
const heart = (s = 1, dx = 0, dy = 0) => xf(HEART, s, dx, dy);
// ring: a ~4 px band, measured pixel by pixel on Oracle's icons
const heartRing = (s = 1, dx = 0, dy = 0) => heart(s, dx, dy) + heart(s * 0.8, dx, dy);
// flame with its own flame hollowed out, like Oracle's "critical"
const flame = (s = 1, dx = 0, dy = 0) =>
  xf(FLAME, s, dx, dy) + xf(FLAME, s * 0.46, dx, dy + s * 7);

// ------------------------------------------------------------- primitives
const circ = (cx, cy, r) =>
  `M${cx - r},${cy}A${r},${r} 0 1,0 ${cx + r},${cy}A${r},${r} 0 1,0 ${cx - r},${cy}Z`;
const poly = pts => 'M' + pts.map(p => p.join(',')).join('L') + 'Z';
const lens = (cx, cy, w, h) =>
  `M${cx - w / 2},${cy}Q${cx},${cy - h / 2} ${cx + w / 2},${cy}`
  + `Q${cx},${cy + h / 2} ${cx - w / 2},${cy}Z`;
// the same lens, pointed at top and bottom (ears)
const vlens = (cx, cy, w, h) =>
  `M${cx},${cy - h / 2}Q${cx + w / 2},${cy} ${cx},${cy + h / 2}`
  + `Q${cx - w / 2},${cy} ${cx},${cy - h / 2}Z`;
const drop = (cx, cy, w, h) => {
  const r = w / 2, by = cy + h / 2 - r;
  return `M${cx},${cy - h / 2}Q${cx + r},${cy - h / 6} ${cx + r},${by}`
       + `A${r},${r} 0 1,1 ${cx - r},${by}Q${cx - r},${cy - h / 6} ${cx},${cy - h / 2}Z`;
};
// Maple leaf: normalised half-profile (tip at 0,-1; petiole base at 0,+1),
// mirrored. Its sharp side points come from the narrowing, where a vine
// leaf's rounded lobes would merge into a blob.
// The NOTCH radius is what decides everything: too short, and each lobe
// becomes a spike and the leaf reads as a star. Here the notches stay
// around 0.6 of the radius while the points reach 0.8-1.0, which leaves a
// central mass and gives the lobes some body.
const MAPLE = [
  [0.00, -1.00], [0.20, -0.62], [0.46, -0.66], [0.44, -0.34],
  [0.74, -0.34], [0.62, -0.10], [1.00, -0.02], [0.56, 0.20],
  [0.62, 0.46], [0.32, 0.42], [0.12, 0.58], [0.05, 1.00],
];
// `petiole` set to false drops the last point: the leaf ends on a straight
// edge instead of a point, and only points downward.
const maple = (cx, cy, w, h, flip = 1, petiole = true) => {
  const pts = petiole ? MAPLE : MAPLE.slice(0, -1);
  const m = ([x, y]) => [+(cx + x * w).toFixed(2), +(cy + flip * y * h).toFixed(2)];
  const droite = pts.map(m);
  const gauche = pts.slice().reverse().slice(0, -1).map(([x, y]) => m([-x, y]));
  return poly([...droite, ...gauche]);
};

const star4 = (cx, cy, R, r) => poly(Array.from({ length: 8 }, (_, k) => {
  const a = k * Math.PI / 4 - Math.PI / 2, rad = k % 2 ? r : R;
  return [+(cx + rad * Math.cos(a)).toFixed(2), +(cy + rad * Math.sin(a)).toFixed(2)];
}));

const fill = (d, col, eo) => `<path d="${d}"${eo ? ' fill-rule="evenodd"' : ''} fill="${col}"/>`;
// scales an arbitrary SVG fragment, around the box's centre
const scaleG = (s, dx, dy, body) =>
  `<g transform="translate(${dx} ${dy}) translate(32 32) scale(${s}) translate(-32 -32)">`
  + `${body}</g>`;
const rot  = (a, cx, cy, body) => `<g transform="rotate(${a} ${cx} ${cy})">${body}</g>`;
const arc  = (cx, cy, r, a0, a1, w, col) => {
  const p = a => `${(cx + r * Math.cos(a * Math.PI / 180)).toFixed(2)},`
               + `${(cy + r * Math.sin(a * Math.PI / 180)).toFixed(2)}`;
  return `<path d="M${p(a0)}A${r},${r} 0 ${Math.abs(a1 - a0) > 180 ? 1 : 0},1 ${p(a1)}"`
       + ` fill="none" stroke="${col}" stroke-width="${w}" stroke-linecap="round"/>`;
};
const line = (d, col, w, cap = 'round') =>
  `<path d="${d}" fill="none" stroke="${col}" stroke-width="${w}" stroke-linecap="${cap}"/>`;

let uid = 0;
const cut = (shape, col, holes, eo) => {           // `holes` is removed from `shape`
  const id = 'm' + (++uid);
  return `<mask id="${id}"><rect width="64" height="64" fill="#fff"/>${holes}</mask>`
       + `<path d="${shape}"${eo ? ' fill-rule="evenodd"' : ''} fill="${col}" mask="url(#${id})"/>`;
};
const K = d => `<path d="${d}" fill="#000"/>`;      // hole


// ===================================================== the two icon sets
// =========================================================== SKILLS
// The twelve vanilla SkillDefs. A SEPARATE set from the passions, and
// deliberately MONOCHROME: in this mod colour already means "which
// passion", and having it also mean "which skill" would make both
// unreadable. Here shape alone carries the identity - which amounts to
// applying the icon set's rule 2 (readable in silhouette) as the sole rule.
//
// Only two tones: a light mass, a shadow for internal detail. At 20 px the
// second tone no longer reads as a colour but as a hollow, which is what
// keeps the icon from flattening into a blob.
const OUTIL = { clair: '#D6D6D6', ombre: '#8F8F8F' };
const mono = k => OUTIL[k];

// gear wheel: n square teeth between inner radius r and outer radius R
const roue = (cx, cy, R, r, n) => poly(Array.from({ length: n * 4 }, (_, k) => {
  const pas = 2 * Math.PI / (n * 4);
  const a = k * pas - Math.PI / 2;
  const rad = (k % 4 === 0 || k % 4 === 3) ? R : r;
  return [+(cx + rad * Math.cos(a)).toFixed(2), +(cy + rad * Math.sin(a)).toFixed(2)];
}));

// speech bubble with its tail
const bulle = (cx, cy, w, h, sens = 1) =>
  `M${cx - w / 2},${cy - h / 2}h${w}a4,4 0 0,1 4,4v${h - 8}a4,4 0 0,1 -4,4`
  + `h${-(w / 2 - 4 * sens)}l${-5 * sens},6l${sens > 0 ? 0.5 : -0.5},-6`
  + `h${-(w / 2 + 4 * sens - 4)}a4,4 0 0,1 -4,-4v${-(h - 8)}a4,4 0 0,1 4,-4Z`;

// -- shooting: a reticle, not a rifle. At 20 px a long gun becomes a
//    horizontal smear - true of almost every icon set, and the reason they
//    all reach for a target. The ring and the four ticks survive any
//    reduction.
I.SK_Shooting = c =>
    cut(circ(32, 32, 21), c('clair'), K(circ(32, 32, 14)))
  + fill('M29,4h6v12h-6Z', c('clair'))                      // ticks
  + fill('M29,48h6v12h-6Z', c('clair'))
  + fill('M4,29h12v6h-12Z', c('clair'))
  + fill('M48,29h12v6h-12Z', c('clair'))
  + fill(circ(32, 32, 5), c('ombre'));

// -- melee: a sword pointing up. The guard is what separates it from a
//    plain triangle, and the pommel what keeps it from floating.
I.SK_Melee = c => rot(35, 32, 32,
    fill('M32,6l4,8v26h-8V14Z', c('clair'))                 // blade
  + fill('M20,41h24v5h-24Z', c('ombre'))                    // guard
  + fill('M29,46h6v9h-6Z', c('clair'))                      // grip
  + fill(circ(32, 57, 4), c('ombre')));                     // pommel

// -- construction: a hammer. The trowel used before read as a downward
//    arrow - a triangle pointing down belongs to nobody. The hammer holds
//    on its head sitting frankly off-centre on the handle: that imbalance
//    is what names it, not the detail.
I.SK_Construction = c => rot(22, 32, 32,
    fill('M12,12h30v16h-30Z', c('clair'))                    // head
  + fill('M42,14l8,4v6l-8,4Z', c('ombre'))                   // peen
  + fill('M22,28h8v28h-8Z', c('clair')));                    // handle

// -- mining: a pickaxe. The symmetrical version read as an UMBRELLA, and it
//    was unassailable: an arc centred on a vertical handle IS an umbrella.
//    Two fixes, both necessary - the head is asymmetric (a point on one
//    side, an edge on the other), and the whole thing is tilted diagonally,
//    an axis no umbrella ever stands on.
I.SK_Mining = c => rot(-28, 32, 32,
    fill('M6,30l24,-9l26,4l-3,7l-23,-2l-22,7Z', c('clair'))  // head
  + fill('M27,26h9v32h-9Z', c('ombre')));                    // handle

// -- cooking: a pot. Two handles, a lid, and steam - without the steam it
//    reads as a bucket.
I.SK_Cooking = c =>
    fill('M14,32h36v14a8,8 0 0,1 -8,8h-20a8,8 0 0,1 -8,-8Z', c('clair'))
  + fill('M10,30h44v5h-44Z', c('clair'))                     // lid
  + fill(circ(32, 26, 3), c('ombre'))                        // knob
  + line('M9,36q-4,3 0,6', c('ombre'), 3)                    // handles
  + line('M55,36q4,3 0,6', c('ombre'), 3)
  + line('M24,20q3,-4 0,-8', c('ombre'), 3)                  // steam
  + line('M40,20q3,-4 0,-8', c('ombre'), 3);

// -- plants: a two-leaved sprout. Two fixes from the first pass: the
//    ground line is dropped, because together with the stem it drew a "T"
//    that ate the whole reading; and the leaves are plumper and tilted,
//    where flat lenses disappeared.
I.SK_Plants = c =>
    line('M32,58q0,-16 0,-26', c('ombre'), 5)
  + rot(-30, 18, 30, fill(lens(18, 30, 28, 22), c('clair')))
  + rot(30, 46, 20, fill(lens(46, 20, 28, 22), c('clair')));

// -- animals: a paw print. Four toes at different tilts, otherwise the paw
//    reads as four aligned dots.
I.SK_Animals = c =>
    fill('M32,52q-14,0 -14,-10q0,-10 14,-10t14,10q0,10 -14,10Z', c('clair'))
  + rot(-18, 17, 24, fill(vlens(17, 24, 10, 15), c('clair')))
  + rot(-6, 26, 18, fill(vlens(26, 18, 10, 16), c('clair')))
  + rot(6, 38, 18, fill(vlens(38, 18, 10, 16), c('clair')))
  + rot(18, 47, 24, fill(vlens(47, 24, 10, 15), c('clair')));

// -- crafting: a gear wheel. The hub must be HOLLOWED, not just darker: at
//    20 px a solid hub turns the wheel into a disc with irregular edges.
I.SK_Crafting = c =>
  cut(roue(32, 32, 26, 20, 8), c('clair'), K(circ(32, 32, 9)));

// -- art: a paintbrush, tilted. The metal ferrule is what sets it apart
//    from a pencil, and the drop says it paints.
I.SK_Artistic = c => rot(35, 32, 32,
    fill('M29,8h6v28h-6Z', c('clair'))                       // handle
  + fill('M27,36h10v7h-10Z', c('ombre'))                     // ferrule
  + fill('M27,43h10l-5,13Z', c('clair')))                    // bristles
  + fill(drop(50, 50, 9, 12), c('ombre'));                   // drop

// -- medicine: a cross with rounded arms. A sharp-cornered cross reads as a
//    plus sign; the rounded joins make it a symbol.
I.SK_Medicine = c =>
  fill('M26,10h12a4,4 0 0,1 4,4v12h12a4,4 0 0,1 4,4v4a4,4 0 0,1 -4,4h-12v12'
     + 'a4,4 0 0,1 -4,4h-12a4,4 0 0,1 -4,-4v-12h-12a4,4 0 0,1 -4,-4v-4'
     + 'a4,4 0 0,1 4,-4h12v-12a4,4 0 0,1 4,-4Z', c('clair'));

// -- social: two speech bubbles answering each other. Their tails point
//    TOWARD each other: the other way round reads as two monologues.
I.SK_Social = c =>
    fill(bulle(24, 24, 30, 20, 1), c('clair'))
  + fill(bulle(40, 42, 26, 18, -1), c('ombre'));

// -- intellectual: a flask. The narrow neck and sloped shoulders set it
//    apart from drunken's tumbler, which flares at the top.
I.SK_Intellectual = c =>
    fill('M27,8h10v16l13,24a6,6 0 0,1 -5,9h-26a6,6 0 0,1 -5,-9l13,-24Z', c('clair'))
  + fill('M24,10h16v4h-16Z', c('ombre'))                     // neck
  + fill(circ(28, 46, 3), c('ombre'))
  + fill(circ(37, 42, 2), c('ombre'));

const SKILLS = ['Shooting', 'Melee', 'Construction', 'Mining', 'Cooking', 'Plants',
                'Animals', 'Crafting', 'Artistic', 'Medicine', 'Social', 'Intellectual'];
const dirSkill = `${base}/_tools/svg/Skills`;
fs.mkdirSync(dirSkill, { recursive: true });
for (const n of SKILLS) {
  uid = 0;
  fs.writeFileSync(`${dirSkill}/${n}.svg`, head + I['SK_' + n](mono) + '</svg>');
  uid = 0;
  fs.writeFileSync(`${dirs.sil}/SK_${n}.svg`, head + I['SK_' + n](() => '#000') + '</svg>');
}
console.log(`${SKILLS.length} skill icons written`);

// ====================================================== WORK TYPES
// The 23 vanilla and DLC WorkTypeDefs. Same rule as the skills: monochrome,
// shape alone.
//
// Nine of them REUSE their skill's drawing. That is not laziness: "Cook"
// and "Cooking" name the same domain, and giving them two different glyphs
// would imply two notions. The fourteen others have no skill - or not the
// same one - and are drawn here.
const MEME_QUE = {
  Art: 'Artistic', Construction: 'Construction', Cooking: 'Cooking',
  Crafting: 'Crafting', Doctor: 'Medicine', Growing: 'Plants',
  Handling: 'Animals', Mining: 'Mining', Research: 'Intellectual',
};

// -- firefighter: a two-tone flame. The solid version read as a WATER DROP,
//    which is the perfect opposite for a firefighter: what makes a flame
//    is not its outline, it is its darker core. Same construction as the
//    passions' "critical", which reads well.
I.WT_Firefighter = c =>
    fill('M34,3q2,14 10,21q9,11 4,23q-4,11 -16,11q-13,0 -17,-11q-4,-11 3,-19'
       + 'q-1,8 3,11q-5,-16 13,-36Z', c('clair'))
  + fill('M33,28q1,7 6,12q4,6 1,12q-3,6 -8,6q-7,0 -8,-7q-1,-6 3,-10'
       + 'q0,4 2,5q-2,-9 4,-18Z', c('ombre'));

// -- basic work: a switch. This work type covers what needs no skill at
//    all - operate, open, shut off.
I.WT_BasicWorker = c =>
    fill('M14,22h36a11,11 0 0,1 0,22h-36a11,11 0 0,1 0,-22Z', c('clair'))
  + fill(circ(42, 33, 8), c('ombre'));

// -- childcare: a baby bottle. The graduation marks are what keeps it from
//    being a plain flask.
I.WT_Childcare = c =>
    fill('M22,28h20v22a7,7 0 0,1 -7,7h-6a7,7 0 0,1 -7,-7Z', c('clair'))
  + fill('M23,21h18v7h-18Z', c('ombre'))
  + fill('M28,7q4,-5 8,0v14h-8Z', c('clair'))
  + fill('M26,36h8v3h-8Z', c('ombre'))
  + fill('M26,43h8v3h-8Z', c('ombre'));

// -- cleaning: a broom. The bristles flare out - a straight rectangle read
//    as a mallet.
I.WT_Cleaning = c => rot(18, 32, 32,
    fill('M29,4h6v30h-6Z', c('ombre'))
  + fill('M22,34h20l6,22h-32Z', c('clair'))
  + fill('M23,42h18v3h-18Z', c('ombre')));

// -- dark study: an eye with a slit pupil. The first pass flattened into a
//    blob and was confused with the fish. Two fixes: the eye is less
//    stretched, and the pupil is a dark DISC split by a light slit - an
//    internal contrast, where a dark slit alone filled the whole eye.
I.WT_DarkStudy = c =>
    fill(lens(32, 32, 46, 34), c('clair'))
  + fill(circ(32, 32, 11), c('ombre'))
  + fill(vlens(32, 32, 6, 20), c('clair'));

// -- fishing: a fish. The triangular tail carries the reading on its own,
//    the eye only says which side the head is on.
I.WT_Fishing = c =>
    fill(lens(27, 32, 38, 26), c('clair'))
  + fill(poly([[44, 32], [58, 21], [58, 43]]), c('clair'))
  + fill(circ(18, 29, 3), c('ombre'));

// -- hauling: a crate and an arrow. Without the arrow, it is storage; with
//    it, it is a move.
I.WT_Hauling = c =>
    fill('M32,4l11,13h-7v7h-8v-7h-7Z', c('clair'))
  + fill('M11,28h42v26h-42Z', c('clair'))
  + fill('M11,37h42v5h-42Z', c('ombre'));

// -- hunting: a drawn bow. The first pass read as a "back" button: the arc
//    bulging left and the point on the right formed a single chevron. The
//    FLETCHING settles it - two barbs at the back give a reading direction
//    no chevron has. The bow is also slimmed down so the arrow dominates,
//    not the other way round.
I.WT_Hunting = c =>
    arc(48, 32, 26, 128, 232, 4, c('ombre'))
  + line('M32,10v44', c('ombre'), 2)
  + fill('M10,30h34v4h-34Z', c('clair'))
  + fill(poly([[40, 24], [58, 32], [40, 40]]), c('clair'))
  + fill(poly([[10, 22], [18, 30], [10, 30]]), c('clair'))
  + fill(poly([[10, 42], [18, 34], [10, 34]]), c('clair'));

// -- patient: a capsule. Receiving care is not giving it - the cross stays
//    with the doctor.
I.WT_Patient = c => rot(-35, 32, 32,
    fill('M16,24h32a10,10 0 0,1 0,20h-32a10,10 0 0,1 0,-20Z', c('clair'))
  + fill('M16,24h16v20h-16a10,10 0 0,1 0,-20Z', c('ombre')));

// -- bed rest: a bed. The headboard and legs are what separate it from a
//    plain horizontal bar.
I.WT_PatientBedRest = c =>
    fill('M6,16h6v30h-6Z', c('clair'))
  + fill('M6,30h50v10h-50Z', c('clair'))
  + fill('M14,21h14v9h-14Z', c('ombre'))
  + fill('M8,40h6v10h-6Z', c('clair'))
  + fill('M48,40h6v10h-6Z', c('clair'));

// -- plant cutting: shears. The two rings say "tool"; two crossed blades
//    alone would make a Saint Andrew's cross.
I.WT_PlantCutting = c =>
    line('M22,8L42,38', c('clair'), 6)
  + line('M42,8L22,38', c('clair'), 6)
  + cut(circ(19, 48, 9), c('ombre'), K(circ(19, 48, 4)))
  + cut(circ(45, 48, 9), c('ombre'), K(circ(45, 48, 4)));

// -- smithing: an anvil. The hammer is already taken by construction, and
//    the anvil says "metal" on its own anyway.
I.WT_Smithing = c =>
    fill('M8,22h34l10,-6v6h4v10h-8l-5,6h-14l-4,-6h-17Z', c('clair'))
  + fill('M24,38h14l9,16h-32Z', c('ombre'));

// -- tailoring: a garment. The spool and thread read as the letter "D" -
//    too many thin strokes for 20 px. The result of the work says the work
//    better than its tool, and a tunic silhouette does not resemble
//    anything else in the icon set.
I.WT_Tailoring = c =>
    fill('M24,10h16l16,9l-5,11l-7,-4v28h-24v-28l-7,4l-5,-11Z', c('clair'))
  + fill('M26,10h12l-6,7Z', c('ombre'));

// -- warden: a key. A cell's bars reduce to parallel lines, unreadable; the
//    key keeps its shape at any size.
I.WT_Warden = c =>
    cut(circ(17, 32, 13), c('clair'), K(circ(17, 32, 6)))
  + fill('M28,28h28v8h-28Z', c('clair'))
  + fill('M42,36h5v9h-5Z', c('ombre'))
  + fill('M52,36h4v7h-4Z', c('ombre'));

const WORKTYPES = ['Art', 'BasicWorker', 'Childcare', 'Cleaning', 'Construction',
  'Cooking', 'Crafting', 'DarkStudy', 'Doctor', 'Firefighter', 'Fishing', 'Growing',
  'Handling', 'Hauling', 'Hunting', 'Mining', 'Patient', 'PatientBedRest',
  'PlantCutting', 'Research', 'Smithing', 'Tailoring', 'Warden'];
const dirWT = `${base}/_tools/svg/WorkTypes`;
fs.mkdirSync(dirWT, { recursive: true });
for (const n of WORKTYPES) {
  const dessin = MEME_QUE[n] ? 'SK_' + MEME_QUE[n] : 'WT_' + n;
  if (!I[dessin]) throw new Error(`no drawing found for work type ${n}`);
  uid = 0;
  fs.writeFileSync(`${dirWT}/${n}.svg`, head + I[dessin](mono) + '</svg>');
  uid = 0;
  fs.writeFileSync(`${dirs.sil}/WT_${n}.svg`, head + I[dessin](() => '#000') + '</svg>');
}
console.log(`${WORKTYPES.length} work type icons written`
  + ` (${Object.keys(MEME_QUE).length} reused from a skill)`);
