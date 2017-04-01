function Player() {
    this.x = 0;
    this.y = 0;
}

Player.prototype.draw = function(context) {
    context.fillRect(this.x, this.y, 32, 32);
};

Player.prototype.moveLeft = function() {
    var Socket = new WebSocket(, [protocal] );
};

Player.prototype.moveRight = function() {
    this.x += 1;
};

Player.prototype.moveUp = function() {
    this.y -= 1;
};

Player.prototype.moveRight = function() {
    this.y += 1;
};