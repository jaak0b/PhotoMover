# Banned test smells

Catalogue of the smells the test suite bans, with the reasoning and what to do instead. Sources:
Meszaros's xUnit Test Patterns, the Google Testing Blog ("Testing on the Toilet" series), and
Khorikov's Unit Testing Principles.

## Tautological test

The expected value is produced by the same logic as the production code: calling a production
helper to build the expectation, re-implementing the pattern expansion inline, or building the
expected destination path the same way the code under test does. The test and the code share
every bug and the test can only fail on a typo. This is the smell behind the hard rule: expected
values are independent literals.

Fix: hardcode the expectation with a provenance outside the test (hand-derived path string,
spec value).

## Change detector

A test that fails whenever the implementation changes, regardless of whether behavior changed:
asserting private call order, internal intermediate values, or committing large snapshots nobody
reads. Google's phrasing: change detectors are "worse than useless", because every refactor pays
a false-alarm tax and people learn to rubber-stamp expectation updates.

Fix: assert observable output through the public interface only.

## Over-mocking (interaction testing)

`mock.Verify(x => x.Foo(...), Times.Once)` that mirrors the calling code verifies "did I write
the code I wrote", not "is the result correct". Mocks are for true boundaries only: filesystem
(`IFileSystem`), network, clock. The FTP protocol tests are deliberately classicist (Detroit
style): they run against real loopback sockets and real temp directories. Do not introduce mocks
of the protocol handler or socket layer.

Fix: call the real collaborators; assert the resulting state or return value.

## Obscure test / mystery guest

The reader cannot tell what correct behavior the test proves without opening the implementation
or an unseen shared fixture. Common forms: an expectation named `ExpectedResult` imported from
elsewhere, a fixture file whose relevant property is undocumented, setup buried in constructor
chains shared across unrelated tests.

Fix: state the relevant fixture facts in the test (or its name), keep literals visible.

## Assertion roulette

Many asserts in one test with no labels: when one fails, nobody can tell which behavior broke.

Fix: one behavior per test; where several fields describe one outcome, keep them together but
make each assertion self-identifying (FluentAssertions `because` messages).

## Conditional logic in tests

`if`, `for`, or `try` branching on the outcome inside a test means the test itself needs testing,
and some branches never run. A data table of cases driving one straight-line body
(`[Theory]` + `[InlineData]`/`[MemberData]`) is fine: the logic generates cases, it does not
branch on results.

## Shared mutable fixtures

A class-level object mutated across tests couples them: order dependence, mysterious failures
when run in isolation. Prefer a local builder or factory method called inside each test.

## DRY over DAMP

Test code optimizes for verifiability by inspection, not for zero duplication. Duplicating three
lines of arrange code is better than a helper that hides which inputs matter. Extract helpers for
mechanics (opening a loopback connection, creating a temp dir), never for meaning (the values
being asserted).
