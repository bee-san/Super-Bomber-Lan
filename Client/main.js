var text = "";
var grid = ""; //JSON.parse(text)["tiles"];
var fov = 13;
var fovradius = Math.floor(fov / 2);
var width = 64; //JSON.parse(text)["width"];
var height = 64; //JSON.parse(text)["height"];
//var centre = [5, 5];
var bombDisabled = false;
var admin = false;
var x;
var y;
const socket = new WebSocket('ws://138.68.178.39:12345');

function render(centre) {
    fovradius = Math.floor(fov / 2);
    y = centre[0] - fovradius;
    x = centre[1] - fovradius;
    $('.gridSquare').each(function(i, obj) {
        var outOfBoundsY = y < 0 || y >= height;
        var outOfBoundsX = x < 0 || x >= width;
        var outOfFOVY = y > centre[0] + fovradius || y < centre[0] - fovradius;
        var outOfFOVX = x > centre[1] + fovradius || x < centre[1] - fovradius;
        if (outOfBoundsY || outOfBoundsX || outOfFOVY || outOfFOVX) {
            $(obj).css("background-color", "white");
            $(obj).css("border", "5px solid white");
            $(obj).css("border-radius", "0px");
        } else {
            if (typeof grid[y][x]["Entities"][0] == "undefined") {
                $(obj).css("background-color", "grey");
                $(obj).css("border", "5px solid grey");
                $(obj).css("border-radius", "0px");
            } else {
                switch (grid[y][x]["Entities"][0]["EntityType"]) {
                    case 0:
                        $(obj).css("background-color", "red");
                        $(obj).css("border", "5px inset red");
                        $(obj).css("border-radius", "5px");
                        break;
                    case 1:
                        $(obj).css("background-color", "black");
                        $(obj).css("border", "5px outset black");
                        $(obj).css("border-radius", "5px");
                        break;
                    case 2:
                        $(obj).css("background-color", "brown");
                        $(obj).css("border", "5px outset brown");
                        $(obj).css("border-radius", "0px");
                        break;
                    case 3:
                        $(obj).css("background-color", "orange");
                        $(obj).css("border", "5px outset brown");
                        $(obj).css("border-radius", "0px");
                        break;
                    default:
                        $(obj).css("background-color", "grey");
                        $(obj).css("border", "5px inset grey");
                        $(obj).css("border-radius", "0px");
                        break;
                }
            }
        }
        if (y == centre[0] && x == centre[1] && !admin) {
            $(obj).css("background-color", "cyan");
            $(obj).css("border", "5px inset blue");
            $(obj).css("border-radius", "100%");
        }
        x += 1;
        if (x == centre[1] + fovradius + 1) {
            x = centre[1] - fovradius;
            y += 1;
        }
    });
    return centre;
};

function resize() {
    var sizeX = $(window).width();
    var sizeY = $(window).height();
    $('#game').attr("width", sizeY);
    $('#game').attr("height", sizeY);
    $('.gridSquare').attr("width", sizeY / 2 / fov);
    $('.gridSquare').attr("height", sizeY / 2 / fov);
    var y = 0;
    var x = 0;
}

function validate(direction) { //checks depending on direction as to
    //what's in the destination space
    y = centre[0];
    x = centre[1];
    switch (direction) {
        case 0:
        case 32: //set bomb
            return grid[y][x]["Entities"][0] == undefined;
            break;
        case 37: //move left
            return grid[y][x - 1]["Entities"][0] == undefined;
            break;
        case 38: //move up
            return grid[y - 1][x]["Entities"][0] == undefined;
            break;
        case 39: //move right
            return grid[y][x + 1]["Entities"][0] == undefined;
            break;
        case 40: //move down
            return grid[y + 1][x]["Entities"][0] == undefined;
            break;
        default: //fail
            return false;
            break;
    }
    return false;
}

function bomb() {
    if (!bombDisabled) {
        grid[centre[0]][centre[1]]["Entities"][0] = {
            "EntityType": 1 //place bomb (1) at current space
        };
        bombDisabled = true;
        setTimeout(function() { //start 4-second timer for bomb
            bombDisabled = false;
        }, 4000);
    }
}

$(document).keydown(function(e) {
    switch (e.which) {
        case 37: // move left
            e.preventDefault();
            if (validate(e.which) || admin) {
                if (centre[1] > 0) { //if within left boundary
                    centre = render([centre[0], centre[1] - 1]); //render with the player's position one space to the left
                    socket.send('{ "op": 5, "data": "LEFT"}');
                    return centre;
                }
            }
            break;
        case 38: // up
            e.preventDefault();
            if (validate(e.which) || admin) {
                if (centre[0] > 0) {
                    centre = render([centre[0] - 1, centre[1]]);
                    socket.send('{ "op": 5, "data": "UP"}');
                }
                return centre;
            }
            break;
        case 39: // right
            e.preventDefault();
            if (validate(e.which) || admin) {
                if (centre[1] < width - 1) {
                    centre = render([centre[0], centre[1] + 1]);
                    socket.send('{ "op": 5, "data": "RIGHT"}');
                }
                return centre;
            }
            break;
        case 40: // down
            e.preventDefault();
            if (validate(e.which) || admin) {
                if (centre[0] < height - 1) {
                    centre = render([centre[0] + 1, centre[1]]);
                    socket.send('{ "op": 5, "data": "DOWN"}');
                }
                return centre;
            }
            break;
        case 0:
        case 32:
            e.preventDefault();
            bomb();
            break;
        default:
            return; // exit this handler for other keys
    }
});

//Resize function
window.onresize = window.onload = function() {
    resize();
}

function opcodeManagement(socket) {
    socket.onmessage = function(evt) {
        var received_msg = JSON.parse(evt.data);
        switch (received_msg["op"]) {
            case 2:
                console.log(received_msg);
                grid = received_msg["data"]["map"]["tiles"];
                width = received_msg["data"]["map"]["width"];
                height = received_msg["data"]["map"]["height"];
                break;
            case 3:
                console.log("3 received. You're now literally God.");
                admin = true;
                break;
            case 4:
                console.log("4 received.");
                console.log(received_msg);
                centre = [received_msg["data"][0]["y"], received_msg["data"][0]["x"]];
                render(centre);
                break;
            case 5:
                var x = received_msg["data"]["x"];
                var y = received_msg["data"]["y"];
                if (received_msg["data"]["direction"] == "Left") {
                    grid[y][x - 1]["Entities"] = [{
                        "EntityType": 0
                    }];
                }
                if (received_msg["data"]["direction"] == "Up") {
                    grid[y - 1][x]["Entities"] = [{
                        "EntityType": 0
                    }];
                }
                if (received_msg["data"]["direction"] == "Down") {
                    grid[y + 1][x]["Entities"] = [{
                        "EntityType": 0
                    }];
                }
                if (received_msg["data"]["direction"] == "Right") {
                    grid[y][x + 1]["Entities"] = [{
                        "EntityType": 0
                    }];
                }
                grid[y][x]["Entities"] = [];
                break;
            case 6:
                console.log("bomb!");
                //add bomb to tile and alert server
                break;
            case 7:
                //???
                console.log("Explosion!");
                break;
            case 8:
                //render new killfeed entry in iframe
                console.log("killfeed!");
                break;
            case 9:
                //render new killfeed entry in iframe
                console.log("killfeed!");
                break;
            case 10:
                //render new killfeed entry in iframe
                console.log("killfeed!");
                break;
            default:

        }
    };
}

$(document).ready(function() {
    //wait 5
    socket.onopen = function() {
        socket.send('{ "op": 1}');
    };
    socket.addEventListener('open', function(event) {
        console.log("Socket opened.")
    });
    setInterval(opcodeManagement(socket), 1000);
});
