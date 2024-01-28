﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class SubscribeAwaitTest
{
    [Fact]
    public void Queue()
    {
        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperations.Queue);

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(3);
        liveList.Should().Equal([100, 200, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void Drop()
    {
        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperations.Drop);

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void Parallel()
    {
        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperations.Parallel);

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100, 200]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100, 200, 300]);

        subject.OnCompleted();
    }
}