function OpenDialogById(id) {
    if (!id?.length) {
        console.error(`Missing or unexpected id '${id}'`);
    }
    const dialog = document.getElementById(id);
    if (!dialog) {
        console.error(`Unable to find dialog with id '${id}'`);
        return;
    }
    dialog.showModal();
}

function CloseDialogById(id) {
    if (!id?.length) {
        console.error(`Missing or unexpected id '${id}'`);
    }
    const dialog = document.getElementById(id);
    if (!dialog) {
        console.error(`Unable to find dialog with id '${id}'`);
        return;
    }
    dialog.close();
}
