namespace VisitTracker.Models
{
    public enum RecordStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }

    public enum MemberWebsiteRole
    {
        Owner,
        Admin,
        Viewer
    }

    public enum ActivityName
    {
        Click = 1,
        WindowBlur = 2,
        WindowFocus = 3,
        ScrollBottom = 4
    }
}
