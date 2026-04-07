// Live Chat Widget - Restaurant Theme
class LiveChatWidget {
    constructor(bookingId) {
        this.bookingId = bookingId;
        this.chatId = null;
        this.isOpen = false;
        this.messageRefreshInterval = null;
        this.init();
    }

    async init() {
        try {
            // Get or create chat session
            const response = await fetch(`/api/chat/session/${this.bookingId}`);
            if (!response.ok) throw new Error('Failed to initialize chat');

            const data = await response.json();
            this.chatId = data.chatId;

            // Create chat UI
            this.createChatUI();

            // Load initial messages
            await this.loadMessages();

            // Start polling for new messages every 3 seconds
            this.messageRefreshInterval = setInterval(() => this.loadMessages(), 3000);
        } catch (error) {
            console.error('Chat initialization error:', error);
        }
    }

    createChatUI() {
        const chatHTML = `
            <button class="chat-toggle-btn" id="chatToggle" onclick="window.liveChat.toggleChat()" title="เปิดแชท">
                💬
            </button>

            <div class="chat-container hidden" id="chatContainer">
                <div class="chat-header">
                    <h4>🍽️ ช่วยเหลือการจอง</h4>
                    <button class="chat-close-btn" onclick="window.liveChat.toggleChat()">✕</button>
                </div>

                <div class="chat-messages" id="chatMessages"></div>

                <div class="chat-input-area">
                    <input type="text" class="chat-input" id="chatInput" placeholder="พิมพ์ข้อความ..." onkeypress="window.liveChat.handleKeyPress(event)">
                    <button class="chat-send-btn" onclick="window.liveChat.sendMessage()">ส่ง</button>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', chatHTML);
    }

    toggleChat() {
        this.isOpen = !this.isOpen;
        const container = document.getElementById('chatContainer');
        const toggle = document.getElementById('chatToggle');

        if (this.isOpen) {
            container.classList.remove('hidden');
            toggle.style.display = 'none';
            document.getElementById('chatInput').focus();
        } else {
            container.classList.add('hidden');
            toggle.style.display = 'flex';
        }
    }

    async loadMessages() {
        try {
            const response = await fetch(`/api/chat/${this.chatId}/messages`);
            if (!response.ok) throw new Error('Failed to load messages');

            const messages = await response.json();
            const messagesContainer = document.getElementById('chatMessages');

            // Clear existing messages (or only add new ones)
            if (messagesContainer.children.length !== messages.length) {
                messagesContainer.innerHTML = '';

                messages.forEach(msg => {
                    const messageEl = document.createElement('div');
                    messageEl.className = `chat-message ${msg.senderType.toLowerCase()}`;
                    messageEl.textContent = msg.content;
                    messagesContainer.appendChild(messageEl);

                    // Mark as read if customer message
                    if (msg.senderType === 'Customer' && !msg.isRead) {
                        fetch(`/api/chat/message/${msg.messageId}/read`, { method: 'PUT' });
                    }
                });

                // Scroll to bottom
                messagesContainer.scrollTop = messagesContainer.scrollHeight;
            }
        } catch (error) {
            console.error('Error loading messages:', error);
        }
    }

    async sendMessage() {
        const input = document.getElementById('chatInput');
        const message = input.value.trim();

        if (!message) return;

        try {
            const response = await fetch(`/api/chat/${this.chatId}/message`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message: message })
            });

            if (!response.ok) throw new Error('Failed to send message');

            input.value = '';
            await this.loadMessages();
        } catch (error) {
            console.error('Error sending message:', error);
            alert('ไม่สามารถส่งข้อความได้');
        }
    }

    handleKeyPress(event) {
        if (event.key === 'Enter') {
            this.sendMessage();
        }
    }

    destroy() {
        if (this.messageRefreshInterval) {
            clearInterval(this.messageRefreshInterval);
        }
        const container = document.getElementById('chatContainer');
        const toggle = document.getElementById('chatToggle');
        if (container) container.remove();
        if (toggle) toggle.remove();
    }
}

// Initialize live chat on page load
document.addEventListener('DOMContentLoaded', function () {
    const bookingId = document.body.getAttribute('data-booking-id');
    if (bookingId) {
        window.liveChat = new LiveChatWidget(bookingId);
    }
});
