function finishGame(gameId) {
    $.ajax({
        url: '',
        data: { id: id }
    }).done(function () {
        alert('Added');
    });
}

promotionType = 3;

function funcForQueen() {
    swal.close();
    promotionType = 3;
    console.log('Queen');
}

function funcForRook() {
    swal.close();
    promotionType = 5;
    console.log('Rook');
}

function funcForKnight() {
    swal.close();
    promotionType = 4;
    console.log('Knight');
}

function funcForBishop() {
    swal.close();
    promotionType = 6;
    console.log('Bishop');
}

function createButtons() {
    var buttons = document.createElement('div');
    var bishopButton = createButton('Bishop', funcForBishop);
    var knightButton = createButton('Knight', funcForKnight);
    var queenButton = createButton('Queen', funcForQueen);
    var rookButton = createButton('Rook', funcForRook);
    buttons.appendChild(bishopButton);
    buttons.appendChild(knightButton);
    buttons.appendChild(queenButton);
    buttons.appendChild(rookButton);
    return buttons;
}

function createButton(text, cb) {
    var button = document.createElement('button');
    button.innerHTML = text;
    button.onclick = cb;
    return button;
}


async function choosePromotionType() {
    await Swal.fire({
        title: "What piece do you want to promote to?",
        html: createButtons(),
        showConfirmButton: false,
        showCancelButton: false
    });
    console.log(promotionType);
    return promotionType;
}