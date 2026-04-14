namespace Battlegrounds.Cli;

/// <summary>
/// Console UI utilities shared across menus.
/// </summary>
internal static class MenuHelpers {

    /// <summary>
    /// Prompts the user for a non-empty string. Repeats until input is given.
    /// </summary>
    public static string PromptString(string prompt, string? defaultValue = null) {
        while (true) {
            if (defaultValue is not null)
                Console.Write($"{prompt} [{defaultValue}]: ");
            else
                Console.Write($"{prompt}: ");

            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) {
                if (defaultValue is not null)
                    return defaultValue;
                Console.WriteLine("  Input cannot be empty.");
                continue;
            }
            return input;
        }
    }

    /// <summary>
    /// Prompts for an integer within an optional [min, max] range. Returns null if the user presses Enter with no input.
    /// </summary>
    public static int? PromptIntOptional(string prompt, int? min = null, int? max = null) {
        while (true) {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
                return null;
            if (!int.TryParse(input, out int value)) {
                Console.WriteLine("  Not a valid number.");
                continue;
            }
            if (min.HasValue && value < min.Value) {
                Console.WriteLine($"  Must be >= {min.Value}.");
                continue;
            }
            if (max.HasValue && value > max.Value) {
                Console.WriteLine($"  Must be <= {max.Value}.");
                continue;
            }
            return value;
        }
    }

    /// <summary>
    /// Prompts for an integer within an optional [min, max] range.
    /// </summary>
    public static int PromptInt(string prompt, int? min = null, int? max = null) {
        while (true) {
            int? value = PromptIntOptional(prompt, min, max);
            if (value.HasValue)
                return value.Value;
            Console.WriteLine("  Input cannot be empty.");
        }
    }

    /// <summary>
    /// Prompts for a float. Returns null if the user presses Enter with no input.
    /// </summary>
    public static float? PromptFloatOptional(string prompt, float? min = null) {
        while (true) {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
                return null;
            if (!float.TryParse(input, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float value)) {
                Console.WriteLine("  Not a valid number.");
                continue;
            }
            if (min.HasValue && value < min.Value) {
                Console.WriteLine($"  Must be >= {min.Value}.");
                continue;
            }
            return value;
        }
    }

    /// <summary>
    /// Presents a numbered list and asks the user to pick one item. Returns null if the list is empty or the user presses
    /// Enter to cancel.
    /// </summary>
    public static T? SelectFromList<T>(string title, IReadOnlyList<T> items, Func<T, string> label,
        bool allowCancel = true) {
        if (items.Count == 0) {
            Console.WriteLine($"  (no {title} available)");
            return default;
        }

        const int pageSize = 20;
        int page = 0;
        int totalPages = (items.Count + pageSize - 1) / pageSize;

        while (true) {
            int start = page * pageSize;
            int end = Math.Min(start + pageSize, items.Count);

            Console.WriteLine();
            Console.WriteLine($"  -- {title} (page {page + 1}/{totalPages}) --");
            for (int i = start; i < end; i++) {
                Console.WriteLine($"  [{i + 1,3}] {label(items[i])}");
            }

            if (totalPages > 1) {
                if (page < totalPages - 1)
                    Console.WriteLine("  [ n] Next page");
                if (page > 0)
                    Console.WriteLine("  [ p] Previous page");
            }
            if (allowCancel)
                Console.WriteLine("  [  ] Cancel (press Enter)");

            Console.Write("  Choice: ");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
                return default;
            if (input.Equals("n", StringComparison.OrdinalIgnoreCase) && page < totalPages - 1) {
                page++;
                continue;
            }
            if (input.Equals("p", StringComparison.OrdinalIgnoreCase) && page > 0) {
                page--;
                continue;
            }
            if (!int.TryParse(input, out int choice) || choice < 1 || choice > items.Count) {
                Console.WriteLine("  Invalid selection.");
                continue;
            }
            return items[choice - 1];
        }
    }

    /// <summary>
    /// Asks a yes/no question. Defaults to false (No).
    /// </summary>
    public static bool Confirm(string prompt) {
        Console.Write($"{prompt} [y/N]: ");
        string? input = Console.ReadLine()?.Trim();
        return input?.Equals("y", StringComparison.OrdinalIgnoreCase) == true;
    }

    public static void Separator() => Console.WriteLine(new string('-', 60));

    public static void Header(string text) {
        Separator();
        Console.WriteLine($"  {text}");
        Separator();
    }

}
