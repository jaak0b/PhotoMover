---
name: writing-marketing-readme
description: Use when writing or revising the README, release announcements, or any outward-facing text meant to attract new users. Evidence-backed rules for converting a technical hobbyist audience.
---

# Writing marketing text for a technical hobbyist audience

These rules come from a research pass (2026-07). Sourcing, stated honestly: the correlational README/popularity study arXiv 2206.10772 backs the structural basics (a one-line purpose, usage and install sections, images, lists, license, and contribution/reference sections all correlate with repo popularity; correlation, not causation). PostHog's own developer-marketing writing backs the anti-hype and specificity rules. A third-party analysis of Tailscale's Hacker News launches (markepear.dev) backs the peer-voice tone claims. The READMEs of ripgrep and fzf are the concrete pattern examples (fzf for hero plus early screenshot, ripgrep for honest limitations and data-driven comparisons). Rules not traceable to those sources are marked as heuristics. These rules apply to the README and any outward-facing text; in-app text is governed by the `writing-ui-guidance` skill instead.

## The register: a knowledgeable peer, not a manual and not an ad

The voice that converts technical hobbyists (here: photographers who care about their file organization) is a third thing between neutral manual prose and marketing copy: a practitioner talking directly to another practitioner. Here is the problem, here is what this does about it.

- First person is an asset when it carries a real technical story (a real maker who hit a real problem — the existing README's "I built PhotoMover to make importing boringly reliable" opening is this pattern; keep it). It becomes a liability the moment it turns into persuasion or self-congratulation.
- Zero hype: no superlatives, no "revolutionary", "effortless", "blazing", "magic". Developers and serious hobbyists detect marketing spin instantly and find it patronizing. Assume the reader is smart.
- Specifics carry the weight that adjectives cannot: exact placeholder syntax (`{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}`), exact camera-facing capabilities ("embedded FTP server on port 21, passive mode"), exact metadata fields read (date taken, camera model, lens). Naming the concrete mechanism is credibility signaling.
- Section split: the hero line and the "why this exists" section may use peer voice and first person. Everything below (how-to steps, how it works, requirements, limitations) is plain technical prose.

## Structure (in order)

1. **Hero**: one factual spec-like sentence stating function, audience, and payoff mechanism. No slogan. The visitor decides in seconds whether to keep reading.
2. **Badges**: build status, license, version. Near-universal in the example READMEs (ripgrep, fzf) and a cheap credibility signal.
3. **Proof above the fold**: a screenshot of the app actually working, plus a concrete output artifact (a code block showing a real resulting folder tree, e.g. `Sony A7C II/2026/07/DSC01234.ARW`). Pattern example: fzf. The "visitors bounce without early proof" framing is practitioner heuristic, not study-backed, but images correlating with popularity is study-backed.
4. **CTA with friction-killing microcopy**: download/build link plus the genuine trust signals ("free, open source, runs entirely on your PC, photos never leave your machine"). Stating local processing as differentiation is a heuristic (it is true and relevant for personal photos), not a case-study finding.
5. **Why / how it works**: the mechanism is itself the selling point when it is not self-evident — EXIF extraction driving a user-defined path pattern, camera-to-PC FTP transfer without vendor cloud software. Explaining it credibly earns the skeptical crowd's trust.
6. **What you get**: name the exact outputs using community terminology (EXIF, RAW/ARW/CR3, passive-mode FTP, grouping rules). Specificity doubles as searchable keywords for the exact phrases hobbyists Google ("Sony camera FTP transfer to PC").
7. **Honest limitations**: a plain section on requirements and what the tool cannot do — Windows-only, needs .NET, single active rule at a time (ripgrep's "Why shouldn't I use ripgrep?" pattern). Owning limits up front reads as expertise, not weakness, and pre-empts the skeptical forum reply.
8. **Quickstart / build / license, plus a contributing or reference section**: plain technical prose. Contribution guidelines and reference/documentation sections correlate with higher popularity in the arXiv study; include at least a short pointer.

## Proof beats claims

- Balanced factual comparisons against the status quo (manual SD-card copying, vendor import software) are strong trust devices; FUD destroys trust (PostHog).
- Heuristics (general practice, not sourced studies): real user quotes beat zero testimonials, but never fabricate and add social proof only as it accrues; do not build the pitch on vanity metrics such as star counts.

## Scannability

Nobody reads top to bottom. Short sections with headers, short paragraphs, bolded key terms, one visible code or output block early. No prose walls.

## Discipline carried over from the project rules

- Honesty: no setup-specific claim stated as a general truth, no capability overclaim. Every number quoted must be a real measured one.
- No AI attribution anywhere, including the README (CLAUDE.md rule #13).
- Terminology heuristic: one term per concept — pick "grouping rule" OR "pattern", "import" OR "transfer", and use it consistently; no invented synonyms.
