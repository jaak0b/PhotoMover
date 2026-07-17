# Unit test review checklist

Apply to every new or changed test under `PhotoMover.Tests/**`, and to the suite when reviewing it.

1. **Read the assertion without the implementation open.** Can you tell what correct behavior it
   proves? If you have to open the class under test (or an unseen fixture) to understand the
   expectation, it is an obscure test.

2. **Would a plausible behavior-preserving refactor break it?** Renaming a private helper,
   reordering internal steps, or swapping an equivalent algorithm must not fail the test. If it
   would, the test is a change detector.

3. **Was the expected value derived independently?** Every expected literal must have a
   provenance outside the test: hand-derived path string, spec value, known file size. Any
   path-building, formatting, or production-helper call producing an expectation is a violation
   of the no-math hard rule.

4. **Grep for `mock.Verify`** (and call-order/`Times` asserts). Any occurrence that mirrors the
   calling code is interaction testing of a non-boundary; rewrite it to assert the resulting
   state. `Verify` is legitimate only when the call IS the observable outcome at a true boundary
   (e.g. "pipeline was invoked with the active rule").

5. **Tolerances and timeouts justified?** Any wait, delay, or timing band states why it is that
   size and was not widened to silence a flaky failure. Prefer condition polling over fixed
   sleeps in socket tests.

6. **Structure:** one behavior per test, name follows `MethodName_State_Expected`,
   arrange-act-assert visible, no conditional logic branching on outcomes, no shared mutable
   fixtures.

7. **For a bug-fix test: was it seen red?** Confirm it was run against the broken code (or the
   code temporarily re-broken) and failed for the intended reason — CLAUDE.md rule #1 requires
   the red output quoted.

8. **Isolation:** no test writes outside `Path.GetTempPath()`; every temp dir and socket is
   cleaned up in Dispose (CLAUDE.md rule #3).
