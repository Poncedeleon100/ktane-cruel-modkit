// Unscrew Maze
$(function () {
    let svg = $("svg.unscrew-maze");

    for (let r = 0; r < 6; r++) {
        for (let c = 0; c < 6; c++) {
            let cell = MakeSvgElem("rect", {
                class: "highlightable", x: (c * 100), y: (r * 100),
                width: 100, height: 100, fill: "transparent"
            });
            svg.append(cell);
        }
    }
    svg.append(MakeSvgElem("rect", { class: "border", x: 5, y: 5, height: 600, width: 600 }));
});

// Polygonal Mapping
const PolygonalMappingTable = [
    [38, 18, 19, 17, 30, 8, 17, 26, 32, 26],
    [43, 3, 10, 19, 17, 47, 20, 26, 29, 37],
    [28, 3, 5, 26, 7, 28, 40, 14, 36, 16],
    [32, 26, 37, 0, 1, 39, 34, 5, 42, 43],
    [28, 36, 40, 19, 32, 35, 0, 12, 48, 43],
    [9, 27, 5, 30, 14, 9, 29, 36, 48, 24],
    [47, 22, 31, 2, 20, 12, 18, 23, 38, 10],
    [6, 38, 48, 9, 27, 29, 9, 13, 46, 35],
    [0, 22, 14, 48, 37, 10, 38, 3, 48, 23],
    [39, 14, 46, 24, 47, 45, 30, 2, 29, 13],
    [45, 40, 17, 35, 46, 42, 35, 28, 31, 13],
    [33, 40, 11, 25, 21, 45, 45, 22, 16, 10],
    [20, 14, 30, 1, 41, 12, 5, 47, 4, 39],
    [33, 44, 32, 23, 11, 3, 0, 0, 27, 7],
    [6, 20, 6, 34, 19, 30, 25, 31, 43, 35],
    [1, 21, 2, 10, 19, 8, 11, 23, 34, 8],
    [44, 41, 9, 42, 15, 4, 42, 3, 16, 13],
    [11, 36, 14, 36, 1, 40, 1, 43, 37, 22],
    [7, 24, 25, 2, 38, 39, 22, 7, 24, 24],
    [8, 33, 47, 2, 33, 25, 21, 41, 12, 17],
    [15, 15, 6, 42, 23, 31, 4, 12, 46, 18],
    [41, 2, 46, 32, 5, 34, 41, 21, 18, 15],
    [16, 25, 0, 13, 1, 37, 31, 27, 29, 33],
    [11, 34, 20, 15, 9, 18, 10, 4, 3, 5],
    [7, 8, 13, 44, 21, 12, 45, 39, 6, 7],
    [28, 4, 27, 6, 4, 44, 11, 8, 16, 44]
];
const PolygonalMappingImgIndexes = [
    "img/Keypad/1-copyright.png",
    "img/Keypad/2-filledstar.png",
    "img/Keypad/3-hollowstar.png",
    "img/Keypad/4-smileyface.png",
    "img/Keypad/5-doublek.png",
    "img/Keypad/6-omega.png",
    "img/Keypad/7-squidknife.png",
    "img/Keypad/8-pumpkin.png",
    "img/Keypad/9-hookn.png",
    "img/Keypad/10-teepee.png",
    "img/Keypad/11-six.png",
    "img/Keypad/12-squigglyn.png",
    "img/Keypad/13-at.png",
    "img/Keypad/14-ae.png",
    "img/Keypad/15-meltedthree.png",
    "img/Keypad/16-euro.png",
    "img/Keypad/17-circle.png",
    "img/Keypad/18-nwithhat.png",
    "img/Keypad/19-dragon.png",
    "img/Keypad/20-questionmark.png",
    "img/Keypad/21-paragraph.png",
    "img/Keypad/22-rightc.png",
    "img/Keypad/23-leftc.png",
    "img/Keypad/24-pitchfork.png",
    "img/Keypad/25-tripod.png",
    "img/Keypad/26-cursive.png",
    "img/Keypad/27-tracks.png",
    "img/Keypad/28-balloon.png",
    "img/Keypad/29-weirdnose.png",
    "img/Keypad/30-upsidedowny.png",
    "img/Keypad/31-bt.png",
    "img/The Cruel Modkit/Symbols/ayin.png",
    "img/The Cruel Modkit/Symbols/brackets.png",
    "img/The Cruel Modkit/Symbols/ethiopic.png",
    "img/The Cruel Modkit/Symbols/katakana.png",
    "img/The Cruel Modkit/Symbols/pi.png",
    "img/The Cruel Modkit/Symbols/reversee.png",
    "img/The Cruel Modkit/Symbols/rparagraph.png",
    "img/The Cruel Modkit/Symbols/rune.png",
    "img/Keypad Sequence/Symbol02.png",
    "img/Keypad Sequence/Symbol14.png",
    "img/Keypad Sequence/Symbol30.png",
    "img/Keypad Sequence/Symbol41.png",
    "img/Keypad Sequence/Symbol53.png",
    "img/The Cruel Modkit/Symbols/tamil.png",
    "img/The Cruel Modkit/Symbols/theta.png",
    "img/The Cruel Modkit/Symbols/triquetra.png",
    "img/The Cruel Modkit/Symbols/yi.png",
    "img/The Cruel Modkit/Symbols/yo.png"
];