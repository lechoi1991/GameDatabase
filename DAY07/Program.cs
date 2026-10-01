using System;
using System.Collections.Generic;
using LiteDB;

namespace DAY07
{
    public class QuestProgress
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string QuestId { get; set; } = "";
        public int KillCount { get; set; } = 0;
        public int TargetCount { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool IsRewardClaimed { get; set; } = false;
        public List<string> RewardIds { get; set; } = new List<string>();
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            using (LiteDatabase database = new LiteDatabase("QuestProgress.db"))
            {
                ILiteCollection<QuestProgress> quests = database.GetCollection<QuestProgress>("quests");

                quests.EnsureIndex(x => x.PlayerId);

                QuestProgress quest = quests.FindOne(x =>
                    x.PlayerId == 1 &&
                    x.QuestId == "GoblinHunt");

                // 퀘스트가 처음 만들어지는 경우
                if (quest == null)
                {
                    quest = new QuestProgress
                    {
                        PlayerId = 1,
                        QuestId = "GoblinHunt",
                        TargetCount = 3,
                        IsCompleted = false,
                        IsRewardClaimed = false,
                        RewardIds = new List<string>
                        {
                            "Potion"
                        }
                    };

                    quests.Insert(quest);
                }

                Console.WriteLine("고블린 퀘스트를 불러왔습니다.");
                Console.WriteLine(
                    "처치 수: " +
                    quest.KillCount +
                    "/" +
                    quest.TargetCount);

                Console.WriteLine(
                    "완료 여부: " +
                    quest.IsCompleted);

                if (quest.IsCompleted)
                {
                    Console.WriteLine("퀘스트 완료!");

                    if (quest.IsRewardClaimed)
                    {
                        Console.WriteLine("보상은 이미 지급되었습니다.");
                    }
                }

                while (true)
                {
                    Console.Write(
                        "1: 고블린 한 마리 처치 | " +
                        "r: 진행 상태 초기화 | " +
                        "q: 종료 > ");

                    string? input = Console.ReadLine();

                    // q 또는 Q → 프로그램 종료
                    if (input == null || input == "q" || input == "Q")
                    {
                        break;
                    }

                    // r 또는 R → 퀘스트 초기화
                    if (input == "r" || input == "R")
                    {
                        quest.KillCount = 0;
                        quest.IsCompleted = false;

                        // 보상 지급 여부도 초기화
                        quest.IsRewardClaimed = false;

                        quests.Update(quest);

                        Console.WriteLine();
                        Console.WriteLine(
                            "고블린 퀘스트 진행 상태를 초기화했습니다.");

                        Console.WriteLine(
                            "처치 수: " +
                            quest.KillCount +
                            "/" +
                            quest.TargetCount);

                        Console.WriteLine(
                            "완료 여부: " +
                            quest.IsCompleted);

                        Console.WriteLine(
                            "보상 지급 여부: " +
                            quest.IsRewardClaimed);

                        Console.WriteLine();

                        continue;
                    }

                    // 1이 아닌 입력
                    if (input != "1")
                    {
                        Console.WriteLine("1, r 또는 q를 입력하세요.");

                        continue;
                    }

                    // 이미 완료된 퀘스트
                    if (quest.IsCompleted)
                    {
                        Console.WriteLine();
                        Console.WriteLine("이미 완료한 퀘스트입니다.");

                        if (quest.IsRewardClaimed)
                        {
                            Console.WriteLine("보상도 이미 지급되었습니다.");
                        }
                        Console.WriteLine();
                        continue;
                    }

                    quest.KillCount++;

                    Console.WriteLine();
                    Console.WriteLine("고블린을 한 마리 처치했습니다.");

                    Console.WriteLine(
                        "처치 수: " +
                        quest.KillCount +
                        "/" +
                        quest.TargetCount);

                    if (quest.KillCount >= quest.TargetCount)
                    {
                        quest.IsCompleted = true;

                        Console.WriteLine();
                        Console.WriteLine("★ 퀘스트를 완료했습니다! ★");

                        if (!quest.IsRewardClaimed)
                        {
                            Console.WriteLine("보상을 지급합니다.");

                            foreach (string rewardId in quest.RewardIds)
                            {
                                Console.WriteLine("보상 획득: " + rewardId);
                            }
                            quest.IsRewardClaimed = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("아직 퀘스트가 완료되지 않았습니다.");
                    }

                    quests.Update(quest);

                    Console.WriteLine();
                }
            }
        }
    }
}