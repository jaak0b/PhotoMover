# Techniques for proving a test works

A test is a claim ("this would catch that bug"). Each technique below is a way to check the
claim instead of assuming it.

## Watch it fail first

The cheapest proof. When writing a test for a bug, run the test against the broken code before
fixing it and confirm it goes red for the expected reason (read the failure message: a timeout or
a setup exception is not the failure you meant). When writing a test for existing correct code,
temporarily re-break the code (flip a comparison, off-by-one a bound, swap the advertised IP back
to loopback) and confirm the test catches it, then revert. A test that has never been seen red is
unverified; it may be asserting nothing.

## Known-seed inputs beat computed oracles

Where a pipeline transforms input to output (EXIF metadata to destination path), generate or pick
the input from known ground truth and assert the pipeline recovers the hardcoded expectation:
a fixture photo whose camera model and date are known becomes an expected literal path string in
the test. The truth is the seed, never a computed expectation, so this satisfies the no-math
rule.

Caveat: a fixture generated with the same wrong assumptions as the code validates the bug. Derive
the expected output by hand from the raw fixture facts, not by running the production formatter.

## Protocol-level tests over real sockets

For the FTP server, unit-testing handlers in isolation misses framing, buffering, and pipelining
bugs. Drive the real `FtpClientHandler` over a loopback `TcpListener` and assert on the actual
response lines — the pipelined-commands test exists precisely because a per-command
`StreamReader` swallowed buffered input while every isolated test passed.

## Timeouts and waits

A wait in a socket or watcher test is a claim about how long the operation legitimately needs.
Prefer polling a condition with a deadline over a fixed `Task.Delay`; never widen a timeout to
make a flaky test pass — a failure inside the old bound is a regression to explain, not a bound
to stretch.
