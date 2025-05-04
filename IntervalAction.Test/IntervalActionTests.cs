// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.IntervalAction.Test;

[TestClass]
public class IntervalActionTests
{
	[TestMethod]
	public async Task ActionExecutesAfterIntervalFromLastCompletion()
	{
		var counter = 0;
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(100),
			ActionInterval = TimeSpan.FromMilliseconds(300),
			Action = () => Interlocked.Increment(ref counter),
			IntervalType = IntervalType.FromLastCompletion
		};

		var actionInstance = IntervalAction.Start(options);

		// Wait long enough to allow multiple executions.
		await Task.Delay(1100).ConfigureAwait(false);
		actionInstance.Stop();

		// Expect at least 3 executions.
		Assert.IsTrue(counter >= 3, $"Expected at least 3 executions, but got {counter}.");

		actionInstance.RethrowExceptions();
	}

	[TestMethod]
	public async Task NoOverlappingExecutions()
	{
		var executions = 0;
		// Simulate a long-running action so that overlapping is prevented.
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(50),
			ActionInterval = TimeSpan.FromMilliseconds(100),
			Action = () =>
			{
				Interlocked.Increment(ref executions);
				// Simulate a long running task.
				Thread.Sleep(500);
			},
			IntervalType = IntervalType.FromLastStart
		};

		var actionInstance = IntervalAction.Start(options);

		// Allow several polling cycles.
		await Task.Delay(1200).ConfigureAwait(false);
		actionInstance.Stop();

		// With a long-running action, overlapping should be prevented resulting in fewer executions.
		Assert.IsTrue(executions <= 3, $"Expected no overlapping executions, but got {executions} executions.");

		actionInstance.RethrowExceptions();
	}

	[TestMethod]
	public async Task RestartResumesExecution()
	{
		var counter = 0;
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(100),
			ActionInterval = TimeSpan.FromMilliseconds(300),
			Action = () => Interlocked.Increment(ref counter),
			IntervalType = IntervalType.FromLastCompletion
		};

		var actionInstance = IntervalAction.Start(options);

		// Allow some executions.
		await Task.Delay(700).ConfigureAwait(false);
		actionInstance.Stop();
		var countAfterStop = counter;

		// Wait to ensure no further execution occurs after stopping.
		await Task.Delay(500).ConfigureAwait(false);
		Assert.AreEqual(countAfterStop, counter, "No executions should occur after stopping.");

		// Restart the polling.
		await actionInstance.RestartAsync().ConfigureAwait(false);
		await Task.Delay(700).ConfigureAwait(false);
		actionInstance.Stop();
		Assert.IsTrue(counter > countAfterStop, "Executions should resume after restarting.");

		actionInstance.RethrowExceptions();
	}

	[TestMethod]
	public void StartNullOptionsThrows()
	{
		Assert.ThrowsException<ArgumentNullException>(() => IntervalAction.Start(null!));
	}

	[TestMethod]
	public void StartNullActionThrows()
	{
		// Arrange
		// Using null-forgiving operator to bypass the compile-time requirement.
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(10),
			ActionInterval = TimeSpan.FromMilliseconds(10),
			Action = null!,
			IntervalType = IntervalType.FromLastCompletion
		};
		Assert.ThrowsException<ArgumentNullException>(() => IntervalAction.Start(options));
	}

	[TestMethod]
	public async Task RestartStopsPreviousPollingTaskAndStartsNewOne()
	{
		// Arrange
		var counter = 0;
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(10),
			ActionInterval = TimeSpan.FromMilliseconds(10),
			Action = () => counter++,
			IntervalType = IntervalType.FromLastStart
		};

		var intervalAction = IntervalAction.Start(options);
		// Allow polling to run for a moment.
		await Task.Delay(50).ConfigureAwait(false);

		// Act: Restart polling
		var oldPollingTask = intervalAction.PollingTask;
		await intervalAction.RestartAsync().ConfigureAwait(false);

		// Assert: The old polling task should have completed.
		Assert.IsTrue(oldPollingTask.IsCompleted);

		// Allow new polling task to run a bit.
		await Task.Delay(30).ConfigureAwait(false);
		Assert.IsTrue(counter > 0);

		intervalAction.Stop();

		intervalAction.RethrowExceptions();
	}

	[TestMethod]
	public async Task StopPollingTaskStopsExecuting()
	{
		// Arrange
		var counter = 0;
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(10),
			ActionInterval = TimeSpan.Zero,
			Action = () => counter++,
			IntervalType = IntervalType.FromLastStart
		};

		var intervalAction = IntervalAction.Start(options);
		// Allow the polling loop to execute a few times.
		await Task.Delay(30).ConfigureAwait(false);

		// Act
		intervalAction.Stop();
		// Await the polling task to ensure the loop has exited.
		await intervalAction.PollingTask.ConfigureAwait(false);
		var counterAfterStop = counter;

		// Wait additional time to verify no further actions are executed.
		await Task.Delay(30).ConfigureAwait(false);
		Assert.AreEqual(counterAfterStop, counter);

		intervalAction.RethrowExceptions();
	}

	[TestMethod]
	public async Task RethrowExceptionsThrowsException()
	{
		// Arrange
		var exceptionMessage = "Test exception message";
		var options = new IntervalActionOptions
		{
			PollingInterval = TimeSpan.FromMilliseconds(10),
			ActionInterval = TimeSpan.Zero,
			Action = () => throw new InvalidOperationException(exceptionMessage),
			IntervalType = IntervalType.FromLastStart
		};
		var intervalAction = IntervalAction.Start(options);
		// Allow the polling loop to execute a few times.
		await Task.Delay(30).ConfigureAwait(false);
		// Act
		var exception = Assert.ThrowsException<InvalidOperationException>(intervalAction.RethrowExceptions);
		Assert.AreEqual(exceptionMessage, exception.Message);
		intervalAction.Stop();
	}
}
