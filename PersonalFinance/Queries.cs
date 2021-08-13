using PersonalFinance.Itemtemplates;
using System;
using System.Collections.Generic;
using System.Text;

namespace PersonalFinance
{
    class Queries
    {
        public static Queries QueryManager => Q.Value;
        private static readonly Lazy<Queries> Q = new Lazy<Queries>(()=>new Queries());
        private static readonly Dictionary<string, string> QsbyString = new Dictionary<string, string>
        {
            {"Summary",
                @"SELECT ToD AS Date
    ,Income
    ,InvestEarnings
    ,Gastos
    ,Total
FROM dbo.vSummary" },
            {"All info",
                @"SELECT  *
  FROM [dbo].[v_Aggregates_Legible]"},
            {"Aggregates",
                @"
WITH selAggs AS (
	SELECT Aggregates.Id,AggregateID,Aggregates.CurrValue
	FROM Aggregates
	JOIN Dates
	ON Aggregates.DateId = Dates.Id
	WHERE GivenDate = @date
)

SELECT selAggs.ID 
	,AggregateNames.Id AS AggID
	,AggregateNames.AggregateName  AS Name
	,selAggs.CurrValue AS Value
	,AggregateNames.Invest
  FROM [dbo].[AggregateNames]
LEFT OUTER JOIN selAggs
ON selAggs.AggregateID = AggregateNames.Id" },
            {"Dates",
                @"SELECT *
  FROM [dbo].Dates"},
            {"Incomes",
                @"WITH selIncomes AS (
	SELECT IncomeId,IncomeAmount 
	FROM Income
	JOIN Dates
	ON Income.DateId = Dates.Id
	WHERE GivenDate = @date
)

SELECT IncomeNames.ID 
	,[Income]
	,IncomeNames.AggregateID
	,IncomeAmount
  FROM [dbo].[IncomeNames]
LEFT OUTER JOIN selIncomes
ON selIncomes.IncomeId = IncomeNames.ID"},
            {"InvsA",
                @"SELECT  AggregateNames.AggregateName AS Name
	   ,[InvDate] AS Date
      ,[InvAmount] AS Amount
  FROM [dbo].[vInvComplete]
  JOIN AggregateNames
  ON AggregateNames.Id = vInvComplete.InvAggregate
  WHERE [InvDate] > @fromd AND [InvDate] <=@tod"},
            {"InvsB",
                @"SELECT  AggregateNames.AggregateName AS Name
	   ,[EndDate] AS Date
      ,-[InvAmount] AS Amount
  FROM [dbo].[vInvComplete]
  JOIN AggregateNames
  ON AggregateNames.Id = vInvComplete.InvAggregate
  WHERE EndDate IS NOT NULL
	AND [EndDate] > @fromd AND [EndDate] <=@tod

UNION ALL

SELECT AggregateNames.AggregateName AS Name
	   ,stop AS Date
      ,-Amount
FROM Interests
JOIN AggregateNames
  ON AggregateNames.Id = Interests.AggregateId
WHERE  stop > @fromd AND stop <=@tod"},
            {"InvsCValue",
                @"SELECT [AggregateName] AS Name
      ,[GivenDate] AS Date
      ,(CASE WHEN [GivenDate]=@tod
				THEN -[CurrValue]
		ELSE [CurrValue]
		END) AS Amount
  FROM [dbo].vAggregatesComplete
  JOIN Dates
  ON Dates.Id = vAggregatesComplete.DateId
  JOIN AggregateNames
  ON AggregateNames.Id = vAggregatesComplete.AggregateId

  WHERE ([GivenDate] = @fromd OR [GivenDate] = @tod)
		AND Invest=1"},
            {"OverallA",
                @"SELECT TOP (1000) GivenDate AS Date
      ,'OverAll' AS Name
      ,SUM([IncomeAmount]) AS Amount
  FROM [dbo].[Income]
  JOIN Dates
  ON Dates.Id = [Income].[DateId]
  WHERE GivenDate > @fromd AND GivenDate <=@tod
  GROUP BY GivenDate"},
            {"OverallB",
                @"SELECT 'OverAll' AS Name
	  , [ToD] AS Date
      ,-[Gastos] AS Amount
  FROM [dbo].[vSummary]
  WHERE [ToD] > @fromd AND [ToD] <=@tod"},
            {"OverallCValue",
                @"SELECT GivenDate AS Date
      ,'OverAll' AS Name
	  ,(CASE WHEN [GivenDate]=@tod
				THEN -SUM([CurrValue])
		ELSE SUM([CurrValue])
		END) AS Amount
  FROM [dbo].[vAggregatesComplete]
  JOIN Dates
  ON Dates.Id = [vAggregatesComplete].[DateId]
  JOIN AggregateNames
  ON AggregateNames.Id = [vAggregatesComplete].[AggregateID]
  WHERE GivenDate = @fromd OR GivenDate =@tod
  GROUP BY GivenDate"},
            {"CreditCards",
                @"SELECT *
  FROM [dbo].[CreditCardNames]"},
            { "CCMove",
                @"SELECT *
  FROM [dbo].[CreditCardMov]"},
            { "Rewards",
                @"SELECT *
  FROM [dbo].[Rewards]"},
            {"Insert_Date",
                @"INSERT INTO dbo.Dates (ID,GivenDate) 
VALUES (@id,@date)"},
            {"Insert_Income",
                @"INSERT INTO [dbo].[Income] (DateId,IncomeId,IncomeAmount) 
VALUES (@dateid,@incomeid,@amount)"},
            {"Update_Income",
                @"UPDATE [dbo].[Income] 
SET    IncomeAmount = @amount
WHERE DateId = @dateid
    AND IncomeId = @incomeid;"},
            {"Insert_CValue",
                @"INSERT INTO [dbo].[Aggregates] ([DateId],[CurrValue],[AggregateID]) 
VALUES (@dateid,@CValue,@AggId)"},
            {"Update_CValue",
                @"UPDATE [dbo].[Aggregates] 
SET    CurrValue = @CValue
WHERE DateId = @dateid
    AND AggregateID = @AggId;"},
            {"Insert_Invest",
                @"INSERT INTO [dbo].[Inv] ([InvAggregate],[InvDate],[InvAmount],[EndDate]) 
VALUES (@invAgg,@invD,@invAm,@endD)"},
            {"Update_Invest",
                @"UPDATE [dbo].[Aggregates] 
SET    InvAggregate = @invAm
WHERE ID = @Id;"},
            {"Insert_Interest",
                @"INSERT INTO [dbo].[Interests] ([start],[stop],[Amount],[AggregateId],[InvId]) 
VALUES (@start,@stop,@Am,@AggId,@InvId)"},
            {"Update_Interest",
                @"UPDATE [dbo].[Aggregates] 
SET    Amount = @Am
WHERE InvId = @InvId AND start = @start;"},
            {"Insert_CCMove",
                @"INSERT INTO [dbo].[CreditCardMov] ([CardId],[MovDate],[Movement]) 
VALUES (@cardId,@mDate,@amnt)"},
            {"Update_CCMove",
                @"UPDATE [dbo].[CreditCardMov]
SET    [Movement] = @amnt
WHERE Id = @id;"},
            {"Insert_CCReward",
                @"INSERT INTO [dbo].[Rewards] ([CardId],[Dateapp],[Amount]) 
VALUES (@cardId,@mDate,@amnt)"},
            {"Update_CCReward",
                @"UPDATE [dbo].[Rewards]
SET    [Amount] = @amnt
WHERE Id = @id;"}
    };
        private Queries() { }
        public string this[string key]
        {
            get => QsbyString[key];
        }
    }
}
