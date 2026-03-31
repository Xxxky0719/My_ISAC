using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageSearchAndCopy
{
    public class BatchJobService
    {
        public record SearchCriteria(string Pid, string? StepNumber);

        public List<SearchCriteria> ParseManualInput(string input)
        {
            var results = new List<SearchCriteria>();
            if (string.IsNullOrWhiteSpace(input)) return results;

            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    string pid = parts[0].Trim().Trim('"');
                    string? step = parts.Length > 1 ? parts[1].Trim().Trim('"') : null;
                    results.Add(new SearchCriteria(pid, step));
                }
            }
            return results;
        }

        public List<SearchCriteria> LoadFromCsv(string filePath)
        {
            var results = new List<SearchCriteria>();
            if (!File.Exists(filePath)) throw new FileNotFoundException("未找到CSV文件", filePath);

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            int startIndex = 0;
            if (lines.Length > 0 && lines[0].Contains("pid", StringComparison.OrdinalIgnoreCase)) startIndex = 1;

            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length > 0)
                {
                    string pid = parts[0].Trim().Trim('"');
                    string? step = parts.Length > 1 ? parts[1].Trim().Trim('"') : null;
                    if (!string.IsNullOrEmpty(pid)) results.Add(new SearchCriteria(pid, step));
                }
            }
            return results;
        }
    }
}