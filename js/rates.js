let rates = [];

// To customize this, the syntax is "rates[x] = y"
// where "x" is the star value and "y" is the pull rate (out of 100)
rates[5] = 10;
rates[4] = 25;
rates[3] = 65;

let cases = {};
let previousRate = 0;

rates.forEach((rate, star) => {
	cases[previousRate + rate] = star;
	previousRate += rate;
});
