1. Player Identity: Id (string), đã trim và normalized case-insensitive bằng ToUpperInvariant()
2. Score bounds. 0~1_000_000_000 (vary per game)
3. Higher/lower score behavior. 
-> Higher : update current score
-> Lower : keep existing score
4. Equal-score behavior. -> keep existing score
5. Tie ordering. -> Score desc → AchievedAt asc → PlayerId asc
6. Active-season submission rule. -> with in >= StartDate and < EndDate
7. Closed-season reopening rule. -> no reopening, throw exception