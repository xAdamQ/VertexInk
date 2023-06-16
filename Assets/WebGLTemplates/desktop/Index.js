function getImg(evt, gameInstance) {
    const file = evt.target.files[0];
    const url = URL.createObjectURL(file);

    console.log(file);
    console.log(url);

    gameInstance.SendMessage("JsMessenger", "GetFile", url);
}
