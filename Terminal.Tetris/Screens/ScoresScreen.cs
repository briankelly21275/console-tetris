using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Game.Framework.Components;
using Terminal.Tetris.Common;
using Terminal.Tetris.Definitions;
using Terminal.Tetris.Resources;

namespace Terminal.Tetris.Screens
{
    public class ScoresScreen : Screen
    {
        private IList<PlayerScoreItem> _letterBoard;

        public ScoresScreen(Game.Framework.Game game) : base(game)
        {
            _letterBoard = new List<PlayerScoreItem>();
        }

        public async Task<bool> ShowLetterBoardAsync(PlayerScoreItem scores, CancellationToken cancellationToken)
        {
            await LoadScoresAsync(scores, cancellationToken);
            bool? playAgain = null;
            while (playAgain == null)
            {
                await DrawAsync(cancellationToken);
                playAgain = await PlayerInputAsync(cancellationToken);
            }

            return await Task.FromResult((bool) playAgain);
        }

        private async Task LoadScoresAsync(PlayerScoreItem scoreItem, CancellationToken cancellationToken)
        {
            // init serializer
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            //load data
            string jsonString;
            var fileName = $"{nameof(Terminal.Tetris)}.letterboard";
            if (File.Exists(fileName))
            {
                jsonString = await File.ReadAllTextAsync(fileName, cancellationToken);
                _letterBoard = JsonSerializer.Deserialize<IList<PlayerScoreItem>>(jsonString, options);
            }

            // join actual scores
            var item = _letterBoard.FirstOrDefault(x =>
                x.Player.Equals(scoreItem.Player, StringComparison.OrdinalIgnoreCase));
            _letterBoard.Remove(item);

            _letterBoard.Add(scoreItem);

            // taking tops
            _letterBoard = _letterBoard
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Score)
                .Take(Constants.MaxTopPlayers)
                .ToList();

            // storing back to file
            jsonString = JsonSerializer.Serialize(_letterBoard, options);
            await File.WriteAllTextAsync(fileName, jsonString, cancellationToken);
        }

        private async Task DrawAsync(CancellationToken cancellationToken = default)
        {
            await Display.ClearAsync(cancellationToken);
            await Display.OutAsync(16, 1, Strings.Name, cancellationToken);
            await Display.OutAsync(27, 1, Strings.Level, cancellationToken);
            await Display.OutAsync(36, 1, Strings.Score, cancellationToken);
            var i = 1;
            foreach (var item in _letterBoard)
            {
                await Display.OutAsync(16, 1 + i, item.Player, cancellationToken);
                await Display.OutAsync(27, 1 + i, 7, item.Level.ToString(), cancellationToken);
                await Display.OutAsync(36, 1 + i, 4, item.Score.ToString(), cancellationToken);
                if (item.IsCurrentPlayer)
                    await Display.OutAsync(40, 1 + i, 3, Strings.CurrentPlayer, cancellationToken);
                i++;
            }

            await Display.OutAsync(13, 23, Strings.PlayAgain, cancellationToken);
        }

        private async Task<bool?> PlayerInputAsync(CancellationToken cancellationToken = default)
        {
            var input = await Keyboard.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Equals(Strings.Yes, StringComparison.OrdinalIgnoreCase))
                    return await Task.FromResult(true);
                if (input.Equals(Strings.No, StringComparison.OrdinalIgnoreCase))
                    return await Task.FromResult(false);
            }

            return await Task.FromResult((bool?) null);
        }
    }
}