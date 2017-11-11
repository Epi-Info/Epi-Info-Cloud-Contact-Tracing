function WildCardCompare(input1, pattern1, input2, pattern2, input3, pattern3, input4, pattern4, input5, pattern5) {
    var singleWildcard = '?';
    var multipleWildcard = '*';

    var inputList = [];
    var patternList = [];

    if (pattern1) inputList.push(input1); patternList.push(pattern1);
    if (pattern2) inputList.push(input2); patternList.push(pattern2);
    if (pattern3) inputList.push(input3); patternList.push(pattern3);
    if (pattern4) inputList.push(input4); patternList.push(pattern4);
    if (pattern5) inputList.push(input5); patternList.push(pattern5);

    for (var i = 0, len = inputList.length; i < len; i++) {
        var not = false;
        var pattern = patternList[i].toLowerCase();
        if (pattern[0] === "~") {
            not = true;
            pattern = pattern.substr(1);
        } else if (pattern === multipleWildcard || pattern === "regex:.*") continue;

        if (not && !inputList[i]) continue;
        if (!inputList[i]) return false;

		var input = inputList[i].toLowerCase();

        if (pattern.startsWith("regex:")) {
            pattern = pattern.substr(6);
            if (not ? regexCompare(input, pattern) : !regexCompare(input, pattern)) return false;
        }
        else {
            if (not ? compare(input, pattern) : !compare(input, pattern)) return false;
        }
    }

    return true;

    function regexCompare(input, pattern) {
        var result = input.match(pattern);
        return result !== null;
    }

    function compare(input, pattern) {
        isPatternMatched = false;
        var inputLength = input.length;
        var patternLength = pattern.length;

        // Stack containing input positions that should be tested for further matching
        //var inputPosStack = new int[(input.Length + 1) * (pattern.Length + 1)];
        var inputPosStack = [];

        // Stack containing pattern positions that should be tested for further matching
        //var patternPosStack = new int[inputPosStack.Length];                      
        var patternPosStack = [];

        // Each true value indicates that input position vs. pattern position has been tested	
        //var pointTested = new bool[input.Length + 1, pattern.Length + 1];       	
        var pointTested = [];
        for (var i = 0; i < input.length + 1; i++) {
            pointTested[i] = [];
            for (var p = 0; p < pattern.length + 1; p++) {
                pointTested[i][p] = false;
            }
        }

        // Points to last occupied entry in stack; -1 indicates that stack is empty
        var stackPos = -1;

        // Position in input matched up to the first multiple wildcard in pattern
        var inputPos = 0;

        // Position in pattern matched up to the first multiple wildcard in pattern
        var patternPos = 0;

        // Match beginning of the string until first multiple wildcard in pattern
        while (inputPos < inputLength && patternPos < patternLength && pattern[patternPos] !== multipleWildcard && (input[inputPos] === pattern[patternPos] || pattern[patternPos] == singleWildcard)) {
            inputPos++;
            patternPos++;
        }

        // Push this position to stack if it points to end of pattern or to a general wildcard
        if (patternPos === patternLength || pattern[patternPos] === multipleWildcard) {
            pointTested[inputPos][patternPos] = true;
            inputPosStack[++stackPos] = inputPos;
            patternPosStack[stackPos] = patternPos;
        }

        // Repeat matching until either string is matched against the pattern or no more parts remain on stack to test
        while (stackPos >= 0 && !isPatternMatched) {
            // Pop input and pattern positions from stack
            inputPos = inputPosStack[stackPos];

            // Matching will succeed if rest of the input string matches rest of the pattern
            patternPos = patternPosStack[stackPos--];

            if (inputPos === inputLength && (patternPos === patternLength || patternPos === patternLength - 1 && pattern[patternPos] === multipleWildcard)) {
                // Reached end of both pattern and input string, hence matching is successful
                isPatternMatched = true;
            }
            else {
                // First character in next pattern block is guaranteed to be multiple wildcard
                // So skip it and search for all matches in value string until next multiple wildcard character is reached in pattern
                for (var curInputStart = inputPos; curInputStart < inputLength; curInputStart++) {
                    var curInputPos = curInputStart;
                    var curPatternPos = patternPos + 1;
                    if (curPatternPos === patternLength) {
                        // Pattern ends with multiple wildcard, hence rest of the input string is matched with that character
                        curInputPos = inputLength;
                    }
                    else {
                        while (curInputPos < inputLength && curPatternPos < patternLength && pattern[curPatternPos] !== multipleWildcard &&
                            (input[curInputPos] === pattern[curPatternPos] || pattern[curPatternPos] === singleWildcard)) {
                            curInputPos++;
                            curPatternPos++;
                        }
                    }

                    // If we have reached next multiple wildcard character in pattern without breaking the matching sequence, then we have another candidate for full match
                    // This candidate should be pushed to stack for further processing
                    // At the same time, pair (input position, pattern position) will be marked as tested, so that it will not be pushed to stack later again
                    if (((curPatternPos === patternLength && curInputPos === inputLength) || (curPatternPos < patternLength && pattern[curPatternPos] === multipleWildcard))
                        && !pointTested[curInputPos][curPatternPos]) {
                        pointTested[curInputPos][curPatternPos] = true;
                        inputPosStack[++stackPos] = curInputPos;
                        patternPosStack[stackPos] = curPatternPos;
                    }
                }
            }
        }
        return isPatternMatched;
    }
}