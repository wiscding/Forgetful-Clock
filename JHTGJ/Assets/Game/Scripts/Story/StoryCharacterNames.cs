namespace JHTGJ.Story
{
    public static class StoryCharacterNames
    {
        public const string Protagonist = "程述";
        public const string Wife = "许薇";
        public const string Doctor = "医生";

        public static string NormalizeSpeakerName(string speakerName)
        {
            if (string.IsNullOrWhiteSpace(speakerName))
                return string.Empty;

            var speaker = speakerName.Trim().TrimEnd('：', ':');
            switch (speaker)
            {
                case "阿述":
                case "主角":
                case "程述":
                    return Protagonist;
                case "许薇":
                case "薇薇":
                    return Wife;
                case "医生":
                    return Doctor;
                default:
                    return speaker;
            }
        }

        public static string FormatCgDialogue(string speakerName, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text;
        }

        public static string FormatStoryDialogue(string speakerName, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var speaker = NormalizeSpeakerName(speakerName);
            if (string.IsNullOrEmpty(speaker))
                return text;

            return $"{speaker}：{text}";
        }
    }
}
