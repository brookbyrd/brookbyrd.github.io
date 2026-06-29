async function loadQuiz(path, containerId) {
    const response = await fetch(path);
    const questions = await response.json();
    const container = document.getElementById(containerId);

    container.innerHTML = questions.map((q, i) => `
        <div class="question-card">
            <div class="question-id">
                Question ${q.id}
            </div>

            <h3>${q.question}</h3>

            <div id="choices-${i}">
                ${q.choices.map(choice => `
                    <div
                        class="choice-card"
                        id="q${i}-${choice.id}"
                        onclick="checkAnswer(${i}, '${choice.id}', '${q.correct_answer}')">

                        <div class="choice-letter">${choice.id}</div>
                        <div class="choice-text">${choice.text}</div>

                    </div>
                `).join("")}
            </div>

            <p id="feedback-${i}" class="feedback"></p>

            <div id="explanation-${i}" class="explanation" style="display:none;">
                <b>Explanation</b><br>
                ${q.explanation}
            </div>
        </div>
    `).join("");
}

function checkAnswer(questionIndex, selected, correct) {
    const cards = document.querySelectorAll(`#choices-${questionIndex} .choice-card`);

    cards.forEach(card => {
        card.classList.remove("selected", "correct", "incorrect");
    });

    const selectedCard = document.getElementById(`q${questionIndex}-${selected}`);
    selectedCard.classList.add("selected");

    const feedback = document.getElementById(`feedback-${questionIndex}`);
    const explanation = document.getElementById(`explanation-${questionIndex}`);

    if (selected === correct) {
        selectedCard.classList.add("correct");
        feedback.innerHTML = "✅ Correct";
        feedback.style.color = "limegreen";
        explanation.style.display = "block";
    } else {
        selectedCard.classList.add("incorrect");
        feedback.innerHTML = "❌ Incorrect — try another answer";
        feedback.style.color = "red";
        explanation.style.display = "none";
    }
}