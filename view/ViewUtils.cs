namespace view;

public static class ViewUtils {
    public static string FormatDate(DateTime? date) {
        return date?.ToShortDateString() ?? "";
    }
}

// Try to keep in alphabetical order
public static class ViewIds {
    public static string AdminErrorFeedWrapper = "admin-error-feed-wrapper";
    
    public static string WishlistAddDialog = "wishlist-add-dialog";
    public static string WishlistAddWrapper = "wishlist-add-wrapper";
    public static string WishlistEditDialog = "wishlist-edit-dialog";
    public static string WishlistEditWrapper = "wishlist-edit-wrapper";
    public static string WishlistList = "wishlist-list";
}