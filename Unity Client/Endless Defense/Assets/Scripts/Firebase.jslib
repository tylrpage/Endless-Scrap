var LibraryFirebaseJS = {
    GetScores: function() {
        try {
            firebase.database().ref("scores").orderByChild('score').limitToLast(10).get().then(function (snapshot) {
                var orderedData = []
                snapshot.forEach(function (child) {
                    orderedData.push(child.val())
                })
                SendMessage('Managers', 'ScoresReceived', JSON.stringify(orderedData.reverse()));
            });
        } catch (error) {
            SendMessage('Managers', 'OnError', error.message);
        }
    },
    PushScore: function(name_ptr, score) {
        var name = Pointer_stringify(name_ptr);
        try {
            firebase.database().ref("scores").push({
                'name': name,
                score: score
            }).then(function (x) {
                SendMessage('Managers', 'ScoreSent');
            })
        } catch (error) {
            SendMessage('Managers', 'OnError', error.message);
        }
    }
}

mergeInto(LibraryManager.library, LibraryFirebaseJS);