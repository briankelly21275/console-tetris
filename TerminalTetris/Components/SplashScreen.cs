﻿using System;
using System.Threading;
using System.Threading.Tasks;
using GameFramework;
using GameFramework.Components;
using GameFramework.EventArgs;

namespace TerminalTetris.Components
{
    public class SplashScreen : DrawableGameComponent
    {
        public SplashScreen(Game game, bool enabled) : base(game, enabled)
        {
        }

        public override Task DrawAsync(object sender, GameUpdateEventArgs args,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task UpdateAsync(object sender, GameUpdateEventArgs args,
            CancellationToken cancellationToken = default)
        {
            var key = await Game.Keyboard.GetKeyAsync(cancellationToken);
            if (key != null)
            {
                await Game.Display.OutAsync(((char)key).ToString(), cancellationToken);
            }
            await base.UpdateAsync(sender, args, cancellationToken);
        }
    }
}