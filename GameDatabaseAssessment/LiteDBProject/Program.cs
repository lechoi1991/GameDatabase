using System;
using System.Collections.Generic;
using LiteDB;



internal static class Program
{
    private static void Main()
    {
        QuestRepository quests = new QuestRepository("QuestProgress.db");
        Console.WriteLine("LiteDB 퀘스트 평가 시작 프로젝트 (QuestProgress.db)");

        while (true)
        {
            Console.WriteLine("\n0 연결 | 1 초기화 | 2 새 퀘스트 | 3 처치 3회");
            Console.WriteLine("4 퀘스트 삭제 | 5 현재 문서 | 6 두 퀘스트 | q 종료");
            Console.Write("선택: ");
            string? choice = Console.ReadLine()?.Trim();
            if (choice == null || choice.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                switch (choice)
                {
                    case "0":
                        quests.CheckConnection();
                        break;
                    case "1":
                        quests.Reset();
                        break;
                    case "2":
                    case "3":
                    case "4":
                    case "6":
                        RunScenario(quests, choice);
                        break;
                    case "5":
                        PrintQuest(quests, "GoblinHunt");
                        break;
                    default:
                        Console.WriteLine("메뉴 번호를 다시 입력하세요.");
                        break;
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("실행 오류: " + error.Message);
            }
        }
    }

    private static void RunScenario(QuestRepository quests, string choice)
    {
        quests.Reset();
        quests.CreateQuest(1, "GoblinHunt");
        if (choice == "3")
        {
            for (int i = 0; i < 3; i++)
            {
                quests.AddKill(1, "GoblinHunt");
            }
        }
        if (choice == "4")
        {
            quests.DeleteQuest(1, "GoblinHunt");
        }
        if (choice == "6")
        {
            quests.CreateQuest(1, "WolfHunt");
            quests.AddKill(1, "WolfHunt");
        }

        PrintQuest(quests, "GoblinHunt");
        if (choice == "6")
        {
            PrintQuest(quests, "WolfHunt");
        }
        if (choice == "3")
        {
            Console.WriteLine("종료 후 다시 실행해 5번으로 저장 상태를 확인하세요.");
        }
    }

    private static void PrintQuest(QuestRepository repository, string questId)
    {
        QuestProgress quest = repository.FindQuest(1, questId);
        if (quest == null)
        {
            Console.WriteLine(questId + ": 퀘스트 문서 없음");
            return;
        }

        Console.WriteLine("플레이어 " + quest.PlayerId + ", 퀘스트 " + quest.QuestId);
        Console.WriteLine("처치 수 " + quest.CurrentKills + "/" + quest.TargetKills
        + ", 완료 " + quest.IsCompleted
        + ", 보상 " + string.Join(", ", quest.Rewards));
    }
}

public sealed class QuestProgress
{    
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string QuestId { get; set; } = "";
        public int CurrentKills { get; set; }
        public int TargetKills { get; set; }
        public bool IsCompleted { get; set; }
        public List<string> Rewards { get; set; } = new List<string>();
}

public sealed class QuestRepository
{
    private readonly string _path;

    public QuestRepository(string path)
    {
        _path = path;
    }

    public LiteDatabase OpenDatabase()
    {
        return new LiteDatabase(_path);
    }

    public void CheckConnection()
    {
        using (LiteDatabase database = OpenDatabase())
        {
            database.GetCollection<QuestProgress>("quests");
            Console.WriteLine("LiteDB 연결 성공");
        }
    }

    public void Reset()
    {
        using (LiteDatabase database = OpenDatabase())
        {
            ILiteCollection<QuestProgress> collection =
                database.GetCollection<QuestProgress>("quests");
            collection.DeleteAll();
            Console.WriteLine("퀘스트 문서를 초기화했습니다.");
        }
    }

    public void CreateQuest(int playerId, string questId)
    {
        using (LiteDatabase database = OpenDatabase())
        {
            ILiteCollection<QuestProgress> collection =
                database.GetCollection<QuestProgress>("quests");
            if (collection.FindOne(x => x.PlayerId == playerId && x.QuestId == questId) != null)
            {
                throw new InvalidOperationException("이미 있는 퀘스트입니다.");
            }

            QuestProgress quest = new QuestProgress
            {
                PlayerId = playerId,
                QuestId = questId,
                CurrentKills = 0,
                TargetKills = 3,
                IsCompleted = false,
                Rewards = new List<string> { "Potion" }
            };
            collection.Insert(quest);
        }
    }

    public QuestProgress FindQuest(int playerId, string questId)
    {
        using (LiteDatabase database = OpenDatabase())
        {
            ILiteCollection<QuestProgress> collection =
                database.GetCollection<QuestProgress>("quests");
            return collection.FindOne(x => x.PlayerId == playerId && x.QuestId == questId);
        }
    }

    public void AddKill(int playerId, string questId)
    {
        using (LiteDatabase database = OpenDatabase())
        {
            ILiteCollection<QuestProgress> collection =
                database.GetCollection<QuestProgress>("quests");
            QuestProgress quest =
                collection.FindOne(x => x.PlayerId == playerId && x.QuestId == questId);
            if (quest == null)
            {
                throw new InvalidOperationException("퀘스트가 없습니다.");
            }

            if (quest.CurrentKills < quest.TargetKills)
            {
                quest.CurrentKills++;
            }
            quest.IsCompleted = quest.CurrentKills >= quest.TargetKills;
            collection.Update(quest);
        }
    }

    public void DeleteQuest(int playerId, string questId)
    {
        using (LiteDatabase database = OpenDatabase())
        {
            ILiteCollection<QuestProgress> collection =
                database.GetCollection<QuestProgress>("quests");
            QuestProgress quest =
                collection.FindOne(x => x.PlayerId == playerId && x.QuestId == questId);
            if (quest != null)
            {
                collection.Delete(quest.Id);
            }
        }
    }
}