# NCAATournamentSimulator
NCAA bracket simulator using Kenpom.com and ESPN's bracketology bracket

This is a small console app which predicts the final four of the NCAA Tournament.

Team data is retreived from KenPom.com using the HTMLAgilityPack API

The bracket comes from ESPN's Bracketology page

---------------------------------------------------------------------------
Currently the game simulator calculate each team's probability using,
- Pyth 
- AdjO
- AdjD
- SOS
from KenPom.com, and also the seed from Joe Lunardi's Bracketology page.

My probability math is as follows, 

  Team's probability = (((17-Seed)+(AdjO-AdjD))*SOS)*Pyth
  
  Some sample probabilities from this as of 3/9/2015
  - 1 Kentucky vs 16 Texas Southern (Kentucky 99%)
  - 8 Cincinatti vs 9 NC State (NC State 55%)
  - 5 North Carolina vs 12 Iona (North Carolina 89%)

After a teams probability is calculated, a game is played by choosing a random number 
If the number is less than the teams probability, they win the game
(The higher the probability, the more likely p > random number)

This process is then done 20 times (or any amount the user sees fit) for each match-up 

