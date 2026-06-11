namespace EsportHub.Domain.Tournaments;

public static class TournamentConstraints
{
    public const int NameMaxLength = 100;
    public const int TeamsRequiredCount = 16;
    public const int GroupsRequiredCount = 4;
    public const int QualifiedTeamsCount = GroupsRequiredCount * GroupConstraints.QualifiedTeamsCount;
    public const int QuarterFinalMatchesCount = QualifiedTeamsCount / 2;
    public const int SemiFinalMatchesCount = QuarterFinalMatchesCount / 2;
}
