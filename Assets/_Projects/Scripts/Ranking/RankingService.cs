using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public enum RankingScope
{
    Weekly,
    World,
    Country
}

public interface IRankingService
{
    RankingResult GetLeaderboard(RankingScope scope);
}

public sealed class RankingResult
{
    public List<RankItemData> entries;
    public RankItemData mine;
}

public static class RankingServices
{
    public static IRankingService Current { get; } = new FakeRankingService();
}

public sealed class FakeRankingService : IRankingService
{
    private const int BotCount = 100;

    private static readonly string[] WorldFirstNames =
    {
        "Liam", "Olivia", "Noah", "Emma", "Mateo", "Mia", "Lucas", "Sofia",
        "Ethan", "Ava", "Leo", "Isla", "Mason", "Luna", "Kai", "Chloe",
        "Owen", "Nora", "Hugo", "Zoe", "Aria", "Theo", "Mila", "Finn"
    };

    private static readonly string[] WorldLastNames =
    {
        "Carter", "Kim", "Silva", "Brown", "Garcia", "Martin", "Wilson", "Lee",
        "Taylor", "Clark", "Lopez", "Walker", "Hall", "Young", "King", "Scott",
        "Baker", "Green", "Adams", "Hill", "Nelson", "Rivera", "Campbell", "Evans"
    };

    private static readonly string[] CountryFamilyNames =
    {
        "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh", "Phan", "Vu",
        "Vo", "Dang", "Bui", "Do", "Ho", "Ngo", "Duong", "Ly"
    };

    private static readonly string[] CountryGivenNames =
    {
        "Minh", "Anh", "Linh", "Trang", "Huy", "Ngoc", "Tuan", "Mai",
        "Khanh", "Thao", "Nam", "Vy", "Quan", "Nhi", "Duc", "Ha",
        "Bao", "Phuong", "Son", "Yen", "Long", "Tram", "Kiet", "My"
    };

    private readonly Dictionary<RankingScope, RankingBotSnapshot> snapshots = new();

    public RankingResult GetLeaderboard(RankingScope scope)
    {
        RankingBotSnapshot snapshot = GetOrCreateSnapshot(scope);
        var entries = new List<RankItemData>(snapshot.bots.Count + 1);

        foreach (RankItemData bot in snapshot.bots)
        {
            entries.Add(bot.Clone());
        }

        RankItemData mine = CreateMineData();
        entries.Add(mine);
        entries.Sort(CompareEntries);

        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].rank = i + 1;
        }

        return new RankingResult
        {
            entries = entries,
            mine = mine
        };
    }

    private RankingBotSnapshot GetOrCreateSnapshot(RankingScope scope)
    {
        string periodKey = GetPeriodKey(scope, DateTime.UtcNow);

        if (snapshots.TryGetValue(scope, out RankingBotSnapshot cached) &&
            IsValid(cached, periodKey))
        {
            return cached;
        }

        string storageKey = GetStorageKey(scope);
        RankingBotSnapshot stored = LoadSnapshot(storageKey);
        if (IsValid(stored, periodKey))
        {
            snapshots[scope] = stored;
            return stored;
        }

        RankingBotSnapshot generated = GenerateSnapshot(scope, periodKey);
        snapshots[scope] = generated;
        SaveSnapshot(storageKey, generated);
        return generated;
    }

    private static RankingBotSnapshot GenerateSnapshot(RankingScope scope, string periodKey)
    {
        int seed = StableHash($"pizza-rush-ranking-{scope}-{periodKey}");
        var random = new System.Random(seed);
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        var bots = new List<RankItemData>(BotCount);
        int currentScore = GetTopScore(scope);

        for (int i = 0; i < BotCount; i++)
        {
            bots.Add(new RankItemData
            {
                id = $"bot-{scope.ToString().ToLowerInvariant()}-{i + 1:D3}",
                name = CreateUniqueName(scope, random, usedNames),
                avatarType = GetAvatar(random),
                score = Mathf.Max(0, currentScore)
            });

            currentScore -= random.Next(35, 90);
        }

        return new RankingBotSnapshot
        {
            periodKey = periodKey,
            bots = bots
        };
    }

    private static string CreateUniqueName(
        RankingScope scope,
        System.Random random,
        HashSet<string> usedNames)
    {
        string candidate;

        do
        {
            candidate = scope == RankingScope.Country
                ? $"{CountryFamilyNames[random.Next(CountryFamilyNames.Length)]} {CountryGivenNames[random.Next(CountryGivenNames.Length)]}"
                : $"{WorldFirstNames[random.Next(WorldFirstNames.Length)]} {WorldLastNames[random.Next(WorldLastNames.Length)]}";
        }
        while (!usedNames.Add(candidate));

        return candidate;
    }

    private static ItemType GetAvatar(System.Random random)
    {
        List<ItemType> avatarTypes = DataManager.Instance.avatarTypes;
        if (avatarTypes == null || avatarTypes.Count == 0)
        {
            return ItemType.Avatar_1;
        }

        return avatarTypes[random.Next(avatarTypes.Count)];
    }

    private static RankItemData CreateMineData()
    {
        return new RankItemData
        {
            id = "player",
            name = DataManager.Instance.playerName.Value,
            avatarType = DataManager.Instance.currentAvatar.Value,
            score = 4500 + DataManager.Instance.Level.Value * 200
        };
    }

    private static int CompareEntries(RankItemData left, RankItemData right)
    {
        int scoreComparison = right.score.CompareTo(left.score);
        return scoreComparison != 0
            ? scoreComparison
            : string.Compare(left.id, right.id, StringComparison.Ordinal);
    }

    private static bool IsValid(RankingBotSnapshot snapshot, string periodKey)
    {
        if (snapshot == null || snapshot.periodKey != periodKey ||
            snapshot.bots == null || snapshot.bots.Count != BotCount)
        {
            return false;
        }

        foreach (RankItemData bot in snapshot.bots)
        {
            if (bot == null || string.IsNullOrWhiteSpace(bot.id) ||
                string.IsNullOrWhiteSpace(bot.name))
            {
                return false;
            }
        }

        return true;
    }

    private static RankingBotSnapshot LoadSnapshot(string storageKey)
    {
        if (!PlayerPrefs.HasKey(storageKey))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<RankingBotSnapshot>(PlayerPrefs.GetString(storageKey));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load ranking snapshot '{storageKey}': {exception.Message}");
            return null;
        }
    }

    private static void SaveSnapshot(string storageKey, RankingBotSnapshot snapshot)
    {
        PlayerPrefs.SetString(storageKey, JsonUtility.ToJson(snapshot));
        PlayerPrefs.Save();
    }

    private static string GetStorageKey(RankingScope scope)
    {
        return GameConstain.PlayerPrefsKey.RankingSnapshotPrefix + scope;
    }

    private static string GetPeriodKey(RankingScope scope, DateTime utcNow)
    {
        if (scope == RankingScope.Weekly)
        {
            int daysSinceMonday = ((int)utcNow.DayOfWeek + 6) % 7;
            return utcNow.Date.AddDays(-daysSinceMonday)
                .ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        }

        return utcNow.ToString("yyyyMM", CultureInfo.InvariantCulture);
    }

    private static int GetTopScore(RankingScope scope)
    {
        return scope switch
        {
            RankingScope.Weekly => 9000,
            RankingScope.World => 12000,
            RankingScope.Country => 7500,
            _ => 9000
        };
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            const uint offset = 2166136261;
            const uint prime = 16777619;
            uint hash = offset;

            foreach (char character in value)
            {
                hash ^= character;
                hash *= prime;
            }

            return (int)hash;
        }
    }

    [Serializable]
    private sealed class RankingBotSnapshot
    {
        public string periodKey;
        public List<RankItemData> bots = new();
    }
}
