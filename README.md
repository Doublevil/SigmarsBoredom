# SigmarsBoredom
 A solver for Sigmar's Garden, a board mini-game in the game Opus Magnum

![A preview gif of what the solver looks like in action](https://github.com/Doublevil/SigmarsBoredom/blob/master/preview.gif)

## What this does

This is a solver for a mini-game in Opus Magnum called Sigmar's Garden. It's a board game where you are presented with an hexagonal board filled with 11x11 hexagonal tiles that are either empty or contain a marble with one of 14 different symbols. The goal is to clear the board by matching certain symbols together according to a set of rules (some symbols can be matched with the same symbols, some have to be matched with their opposite, some require matching in a specific order, etc). You can only match tiles that both have 3 contiguous empty neighbor tiles.

What this project does is read the screen and take control of your mouse cursor to automatically solve the mini-game.

## How to run it

The project uses win32 APIs. It will only run on Windows. It has only been tested on Windows 10.

Clone the repository, compile with Visual Studio and you should be able to run it.

The solver expects you to have Opus Magnum up and running, with Sigmar's Garden opened on an initial, untouched board.

The game must be launched in borderless window mode (this should be on by default) and at a resolution of 1920x1080.

## How it works

There are several challenges to solve on this. The program first has to read the screen to assess the board and figure out what every tile contains. It then has to figure out a solution for the board. Finally, it has to input the solution (click the right tiles in the game in order).

In order to capture the screen and input mouse events, the solver makes use of win32 APIs.

### Reading the board

Reading the board is the first thing to do, but it's also deceptively hard.

First, I've decided to apply a coordinate system to the grid, with an horizontal offset on odd rows because the tiles are hexagonal. This allows me to rationalize every tile on a captured image as a rectangle that can be precisely identified on the 11x11 board. The rough picture below shows my approach on a coordinate system.

![A visual representation of the coordinate system](https://github.com/Doublevil/SigmarsBoredom/blob/master/Grid.jpg)

Now we need to identify each tile to see if it's empty or if it has a marble, and if so, what marble it is (again, there are 14 kinds of marbles). What is probably not apparent for the casual player is that each tile on the board isn't lit in the same way. There's a lighting effect applied on the whole board that makes it render every tile a bit different from each other. It also affects the reflections you can see on the marbles. This makes it very much harder for us to figure out what's on a tile, because we can't use basic image matching or simple color rules. Moreover, marbles that cannot be played in the current state of the board are heavily faded out, making it harder to automatically differentiate between each symbol and even between empty tiles and tiles with marbles.

After trying several solutions, I've decided to use a feature of the game that highlights marbles with the same symbol when you click on the little icons below the board. So the BoardReader, the component that reads the board, will click on every symbol icon, take a capture of the game with the highlighting for the specific symbol on, and browse each tile of the grid to try and find the bright border that highlights the corresponding marbles.

But then we have another issue: there are no highlighting icons for the arrow symbols (vitae and mors). So we have to identify these separately. I decided to browse the grid separetely for each of these two once we're done with the identification of every other symbol. This allows us to skip tiles that have already been identified as one of the other marbles. This makes the work a lot easier because now we just have to tell if the tile is empty, if it's a vitae, or if it's a mors. We do that with a quick check on the brightness of certain pixels that I manually identified to be relevant.

Once everything is done, we have our 11x11 array populated with tiles that have one of the 15 possible values (the 14 symbols + empty). Before returning that result, the BoardReader checks that our board contains the right number of each marble.

### Solving the game

Solving the game isn't too hard. A board usually has a lot of possible solutions and we only have to find one.

I opted for a depth-first algorithm that lists the possible moves, and for each move recursively lists the possible moves on a board where the previous move has already been played, until either we reached a dead-end, or we cleared the board. If the board is cleared, the solution parts are appended together and returned so that the solution input method can used them.

This approach has execution times that wildly varies depending on how the board is organized, especially in the beginning. As the number of potential moves rises, the number of branches to explore rises exponentially. However, due to the forgiving nature of the game, most of the time, we will be able to quickly find a solution, just because there are lots of possible solutions.

After taking a naïve approach and watching it take several minutes on certain boards, I tried optimizing it a bit to mitigate that. I retained two optimizations: the first one is to assign a very simple priority score to the potential moves, that depends on what sort of combination it makes. For instance, when it has the choice, it will first explore solutions that combine quicksilver marbles early, because it seems to generally be a good idea even when playing casually. The second optimization is to analyze the number of elemental marbles and salt marbles after each move to determine whether the game can still be won or not. These tricks considerably improved the solving times on the most complex boards.

In the current implementation, there are still boards that require several minutes of analysis for the program to solve, but the majority of them are solved almost instantly.

### Inputting the solution

Once we have a solution, all that's left to do is to actually execute the moves. The solver will output the solution as an array containing an object that is a set of two points on the grid. Once we have that, we just have to convert the grid coordinates to actual coordinates in the game's window, move the mouse cursor there, click the first marble, then the second one, go to the next move and repeat until the last step.

I just had to make sure the clicks go through, which was kind of a pain (I had to introduce delays in the code between each win32 API call so that the game gets some time to register everything.

I decided that the solver will be run in an infinite loop. Once it's done inputting a solution, it automatically clicks the "new game" button, waits for the board animation to complete, and start over.