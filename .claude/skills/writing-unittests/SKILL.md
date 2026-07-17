---
name: writing-unittests
description: Use when adding or changing an xUnit test under PhotoMover.Tests/**, when reviewing tests, or when a code change needs a test that would actually catch a real bug, before committing the change.
---

# Writing unit tests

The project has two test styles inside `PhotoMover.Tests`: unit tests (Moq fakes for Core
interfaces, pure logic) and integration-style tests (real loopback sockets, real temp
directories, cleaned up in Dispose — e.g. `FtpClientHandlerTests`). This skill governs both.

## The two questions every test must pass

A test earns its place by answering yes to both (Khorikov's four pillars, Beck's test
desiderata, the Google Testing Blog agree on the core):

1. **Would it fail on a real bug?** (regression protection)
2. **Would it stay green through a behavior-preserving refactor?** (refactoring resistance)

Plus fast feedback and maintainability. Test observable behavior through the public interface of
the class under test; never assert internal steps, private helpers, or call sequences. Test
state, not interactions: an interaction test checks how a result was reached, and only the result
matters. A test that fails both questions is a change detector, and a change detector is worse
than no test, because it trains people to update expectations on sight.

## HARD RULE: no math in tests

An expected value is NEVER calculated inside a test: no formulas, no path-building with the same
helpers production uses, no reuse of production code to derive the expectation. Every expected
result is a hardcoded literal with an independent provenance: hand-derived once outside the test
or taken from a spec. A test that recomputes the expected value with the same logic as production
(e.g. building the expected destination path by calling the same pattern-expansion the
`GroupingRuleEngine` uses) is tautological: it shares any bug with the code under test and can
never catch the bug it mirrors. Write the expected path as a literal string.

## Banned smells

Full catalogue with examples in [references/smells.md](references/smells.md). The headline list
(Meszaros's xUnit Test Patterns, Google Testing Blog):

- **Tautological test**: expected value derived by the production logic (see hard rule above).
- **Change detector**: fails on any implementation change; unread snapshot dumps.
- **Over-mocking**: `mock.Verify(x => x.Foo(...))` mirroring the calling code tests
  "did I write this code". Mock only true boundaries (filesystem, network, clock). Protocol
  tests use real loopback sockets and real temp dirs (the classicist style); keep it that way.
- **Obscure test / mystery guest**: the expected behavior is unreadable without opening the
  implementation or an unseen shared fixture.
- **Assertion roulette**: many unlabeled asserts in one test — use FluentAssertions `because`
  messages where several asserts share a test.
- **Conditional logic in tests**: no if/loops branching on outcomes; `[Theory]`/`[InlineData]`
  case tables are fine.
- **Shared mutable fixtures**: prefer local builders per test.
- **DRY over DAMP**: duplication is acceptable when it keeps a test verifiable by inspection.

## Proving a test works

Details and recipes in [references/techniques.md](references/techniques.md).

- **Watch it fail first.** When writing a test for a bug, run it against the broken code (or
  temporarily re-break it) and see red. A test never seen red is unverified. This is also
  CLAUDE.md rule #1: quote the red output before touching production code.
- **Temporarily re-break** existing correct code (flip a comparison, off-by-one a bound) to
  confirm a new test for it actually catches the mutation, then revert.

## Structure rules

- One behavior per test; the name follows `MethodName_State_Expected` and states the behavior,
  per CLAUDE.md testing conventions.
- Arrange-act-assert visibly separated.
- Expected literals visible in the test body, not hidden behind helpers or constants files.
- Tests touch only `Path.GetTempPath()` dirs and in-memory/loopback resources, cleaned up in
  Dispose (CLAUDE.md rule #3).

## Review checklist

Use [references/review-checklist.md](references/review-checklist.md) when reviewing new or
existing tests.
