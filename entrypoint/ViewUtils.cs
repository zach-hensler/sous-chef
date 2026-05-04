namespace sous_chef;

public static class ViewUtils {
    public static string FormatDate(DateTime? date) {
        return date?.ToShortDateString() ?? "";
    } 
}