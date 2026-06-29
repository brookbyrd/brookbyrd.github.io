let flashcards = [];
let currentCard = 0;
let showingAnswer = false;

async function loadFlashcards(path, containerId) {
    const response = await fetch(path);
    flashcards = await response.json();

    renderFlashcard(containerId);
}

function renderFlashcard(containerId) {
    const container = document.getElementById(containerId);
    const card = flashcards[currentCard];

    container.innerHTML = `
        <div class="flashcard-header">
            <div class="flashcard-id">
                Card ${card.id}
            </div>

            <div class="flashcard-topic">
                ${card.topic} • ${card.difficulty}
            </div>
        </div>

        <div class="quizlet-card" onclick="flipFlashcard('${containerId}')">
            <div class="quizlet-card-content">
                ${showingAnswer ? card.back : card.front}
            </div>
        </div>

        <div class="flashcard-controls">
            <button onclick="previousFlashcard('${containerId}')">← Previous</button>
            <span>${currentCard + 1} / ${flashcards.length}</span>
            <button onclick="nextFlashcard('${containerId}')">Next →</button>
        </div>
    `;
}

function flipFlashcard(containerId) {
    showingAnswer = !showingAnswer;
    renderFlashcard(containerId);
}

function nextFlashcard(containerId) {
    currentCard = (currentCard + 1) % flashcards.length;
    showingAnswer = false;
    renderFlashcard(containerId);
}

function previousFlashcard(containerId) {
    currentCard = (currentCard - 1 + flashcards.length) % flashcards.length;
    showingAnswer = false;
    renderFlashcard(containerId);
}