mergeInto(LibraryManager.library, {
    
    // 1. C#에서 토큰을 요청하면, 브라우저의 LocalStorage에서 토큰을 가져와 반환합니다.
    GetAuthTokenFromBrowser: function (keyPtr) {
        // C#에서 넘어온 문자열 포인터를 JavaScript 문자열로 변환
        var key = UTF8ToString(keyPtr);
        
        // LocalStorage에서 값을 읽어옵니다.
        var token = localStorage.getItem(key);
        
        if (token === null) {
            // 토큰이 없으면 null 대신 빈 문자열을 반환하여 C#에서 쉽게 처리하도록 합니다.
            token = ""; 
        }

        // JavaScript 문자열을 다시 C# 문자열 포인터로 변환하여 반환
        var buffer = _malloc(token.length + 1);
        stringToUTF8(token, buffer, token.length + 1);
        return buffer;
    },
    
    // 2. C#에서 토큰과 키를 넘겨주면, 브라우저의 LocalStorage에 토큰을 저장합니다.
    SetAuthTokenToBrowser: function (keyPtr, tokenPtr) {
        var key = UTF8ToString(keyPtr);
        var token = UTF8ToString(tokenPtr);
        
        // LocalStorage에 토큰 저장
        localStorage.setItem(key, token);
        console.log("인증 토큰이 브라우저 LocalStorage에 저장되었습니다.");
    },

    // 3. 브라우저 탭 닫힘 이벤트 감지 (C#의 최종 저장 함수 호출용)
    // C#에서 이 함수를 호출하면, JS는 window.onbeforeunload 이벤트를 등록합니다.
    RegisterOnBeforeUnload: function (objectNamePtr, methodNamePtr) {
        var objectName = UTF8ToString(objectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);

        window.onbeforeunload = function() {
            // 브라우저가 닫히기 직전, Unity의 특정 함수를 호출하여 최종 저장 요청
            // Unity Loader가 로드된 경우에만 호출 (안정성 확보)
            if (window.unityInstance) {
                window.unityInstance.SendMessage(objectName, methodName);
            }
            // (참고: 비동기 작업 완료를 기다려주지 않으므로, 저장 로직은 매우 빨라야 합니다.)
        };
        console.log("브라우저 닫힘 이벤트 리스너 등록 완료.");
    }
});