﻿using Pigeon.SignalR;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pigeon.Actor;
using System.Collections.Concurrent;

namespace Pigeon.Actor
{
    public class ActorSystem : ActorRefFactory, IDisposable
    {
       
        public ActorSystem()
        {
        }

        protected ConcurrentDictionary<string, ActorRef> Children = new ConcurrentDictionary<string, ActorRef>();

        public override ActorRef ActorOf<TActor>(string name = null)
        {
            name = name ?? typeof(TActor).Name;
            if (name.EndsWith("Actor"))
                name = name.Substring(0, name.Length - 5);

            var existing = Child(name);
            if (existing != null)
                return existing;

            var context = new ActorContext
            {
                System = this,
                Self = new LocalActorRef(new ActorPath(name))
            };
            Children.TryAdd(name, context.Self);
            var actor = (ActorBase)Activator.CreateInstance(typeof(TActor), new object[] { context });
            return context.Self;
        }
        public override ActorRef Child(string name)
        {
            ActorRef actorRef = null; ;
            Children.TryGetValue(name, out actorRef);
            return actorRef;
        }

        public ActorRef ActorSelection(string remoteActorPath,ActorBase owner = null)
        {
            var actorRef = new RemoteActorRef(this, remoteActorPath);
            return actorRef;
        }

        public void Dispose()
        {            
        }

        public override ActorRef ActorSelection(string remoteActorPath)
        {
            throw new NotImplementedException();
        }

        public override void Stop(ActorRef actor)
        {
            ActorRef value = null;
            Children.TryRemove(actor.Path.Name, out value);
        }
    }
}