﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Transport.Channels.Embedded
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;

    sealed class EmbeddedEventLoop : AbstractScheduledEventExecutor, IChannelHandlerInvoker, IEventLoop
    {
        readonly Queue<IRunnable> tasks = new Queue<IRunnable>(2);

        public IEventExecutor Executor => this;

        public IChannelHandlerInvoker Invoker => this;

        public Task RegisterAsync(IChannel channel) => channel.Unsafe.RegisterAsync(this);

        public override bool IsShuttingDown => false;

        public override Task TerminationCompletion
        {
            get { throw new NotSupportedException(); }
        }

        public override bool IsShutdown => false;

        public override bool IsTerminated => false;

        public override bool IsInEventLoop(Thread thread) => true;

        public override void Execute(IRunnable command)
        {
            if (command == null)
            {
                throw new NullReferenceException("command");
            }
            this.tasks.Enqueue(command);
        }

        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        internal PreciseTimeSpan NextScheduledTask() => this.NextScheduledTaskNanos();

        internal void RunTasks()
        {
            for (;;)
            {
                // have to perform an additional check since Queue<T> throws upon empty dequeue in .NET
                if (this.tasks.Count == 0)
                {
                    break;
                }
                IRunnable task = this.tasks.Dequeue();
                if (task == null)
                {
                    break;
                }
                task.Run();
            }
        }

        internal PreciseTimeSpan RunScheduledTasks()
        {
            PreciseTimeSpan time = GetNanos();
            for (;;)
            {
                IRunnable task = this.PollScheduledTask(time);
                if (task == null)
                {
                    return this.NextScheduledTaskNanos();
                }
                task.Run();
            }
        }

        /// <summary>
        ///     YOLO
        /// </summary>
        internal new void CancelScheduledTasks() => base.CancelScheduledTasks();

        public void InvokeChannelRegistered(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeChannelRegisteredNow(ctx);

        public void InvokeChannelUnregistered(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeChannelUnregisteredNow(ctx);

        public void InvokeChannelActive(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeChannelActiveNow(ctx);

        public void InvokeChannelInactive(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeChannelInactiveNow(ctx);

        public void InvokeExceptionCaught(IChannelHandlerContext ctx, Exception cause) => ChannelHandlerInvokerUtil.InvokeExceptionCaughtNow(ctx, cause);

        public void InvokeUserEventTriggered(IChannelHandlerContext ctx, object evt) => ChannelHandlerInvokerUtil.InvokeUserEventTriggeredNow(ctx, evt);

        public void InvokeChannelRead(IChannelHandlerContext ctx, object msg) => ChannelHandlerInvokerUtil.InvokeChannelReadNow(ctx, msg);

        public void InvokeChannelReadComplete(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeChannelReadCompleteNow(ctx);

        public void InvokeChannelWritabilityChanged(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeChannelWritabilityChangedNow(ctx);

        public Task InvokeBindAsync(IChannelHandlerContext ctx, EndPoint localAddress) => ChannelHandlerInvokerUtil.InvokeBindNowAsync(ctx, localAddress);

        public Task InvokeConnectAsync(IChannelHandlerContext ctx, EndPoint remoteAddress, EndPoint localAddress) => ChannelHandlerInvokerUtil.InvokeConnectNowAsync(ctx, remoteAddress, localAddress);

        public Task InvokeDisconnectAsync(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeDisconnectNowAsync(ctx);

        public Task InvokeCloseAsync(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeCloseNowAsync(ctx);

        public Task InvokeDeregisterAsync(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeDeregisterNowAsync(ctx);

        public void InvokeRead(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeReadNow(ctx);

        public Task InvokeWriteAsync(IChannelHandlerContext ctx, object msg) => ChannelHandlerInvokerUtil.InvokeWriteNowAsync(ctx, msg);

        public void InvokeFlush(IChannelHandlerContext ctx) => ChannelHandlerInvokerUtil.InvokeFlushNow(ctx);
    }
}