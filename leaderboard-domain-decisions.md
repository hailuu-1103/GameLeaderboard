1. Player Identity: Id (int)
2. Score bounds. 1~1_000_000_000 (vary per game)
3. Higher/lower score behavior. 
-> Higher : update current score
-> Lower : keep existing score
4. Equal-score behavior. -> keep existing score
5. Tie ordering. -> 1,2,3,...
6. Active-season submission rule. -> with in > StartDate and < EndDate
7. Closed-season reopening rule. -> no reopening, throw exception