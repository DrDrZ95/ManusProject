
import { ChatSession, Role } from '../types';

const NOW = Date.now();
const HOUR = 3600 * 1000;

export const MOCK_SESSIONS: ChatSession[] = [
  {
    id: 'mock-session-1',
    title: 'Python Snake Game',
    updatedAt: NOW - HOUR * 2,
    messages: [
      {
        id: 'm1-1',
        role: Role.USER,
        content: 'Can you help me write a simple Snake game in Python using pygame?',
        timestamp: NOW - HOUR * 2 - 60000
      },
      {
        id: 'm1-2',
        role: Role.MODEL,
        content: `Certainly! Here is a complete, single-file implementation of the classic Snake game using the \`pygame\` library.

### Prerequisites
You will need to install pygame first:
\`\`\`bash
pip install pygame
\`\`\`

### Python Snake Game Code

\`\`\`python
import pygame
import time
import random

pygame.init()

# Colors
WHITE = (255, 255, 255)
YELLOW = (255, 255, 102)
BLACK = (0, 0, 0)
RED = (213, 50, 80)
GREEN = (0, 255, 0)
BLUE = (50, 153, 213)

# Display Dimensions
WIDTH = 600
HEIGHT = 400

dis = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption('Snake Game by Agent')

clock = pygame.time.Clock()

SNAKE_BLOCK = 10
SNAKE_SPEED = 15

font_style = pygame.font.SysFont("bahnschrift", 25)
score_font = pygame.font.SysFont("comicsansms", 35)

def your_score(score):
    value = score_font.render("Your Score: " + str(score), True, YELLOW)
    dis.blit(value, [0, 0])

def our_snake(snake_block, snake_list):
    for x in snake_list:
        pygame.draw.rect(dis, GREEN, [x[0], x[1], snake_block, snake_block])

def message(msg, color):
    mesg = font_style.render(msg, True, color)
    dis.blit(mesg, [WIDTH / 6, HEIGHT / 3])

def gameLoop():
    game_over = False
    game_close = False

    x1 = WIDTH / 2
    y1 = HEIGHT / 2

    x1_change = 0
    y1_change = 0

    snake_List = []
    Length_of_snake = 1

    foodx = round(random.randrange(0, WIDTH - SNAKE_BLOCK) / 10.0) * 10.0
    foody = round(random.randrange(0, HEIGHT - SNAKE_BLOCK) / 10.0) * 10.0

    while not game_over:

        while game_close == True:
            dis.fill(BLUE)
            message("You Lost! Press C-Play Again or Q-Quit", RED)
            your_score(Length_of_snake - 1)
            pygame.display.update()

            for event in pygame.event.get():
                if event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_q:
                        game_over = True
                        game_close = False
                    if event.key == pygame.K_c:
                        gameLoop()

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                game_over = True
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_LEFT:
                    x1_change = -SNAKE_BLOCK
                    y1_change = 0
                elif event.key == pygame.K_RIGHT:
                    x1_change = SNAKE_BLOCK
                    y1_change = 0
                elif event.key == pygame.K_UP:
                    y1_change = -SNAKE_BLOCK
                    x1_change = 0
                elif event.key == pygame.K_DOWN:
                    y1_change = SNAKE_BLOCK
                    x1_change = 0

        if x1 >= WIDTH or x1 < 0 or y1 >= HEIGHT or y1 < 0:
            game_close = True
        x1 += x1_change
        y1 += y1_change
        dis.fill(BLACK)
        pygame.draw.rect(dis, RED, [foodx, foody, SNAKE_BLOCK, SNAKE_BLOCK])
        snake_Head = []
        snake_Head.append(x1)
        snake_Head.append(y1)
        snake_List.append(snake_Head)
        if len(snake_List) > Length_of_snake:
            del snake_List[0]

        for x in snake_List[:-1]:
            if x == snake_Head:
                game_close = True

        our_snake(SNAKE_BLOCK, snake_List)
        your_score(Length_of_snake - 1)

        pygame.display.update()

        if x1 == foodx and y1 == foody:
            foodx = round(random.randrange(0, WIDTH - SNAKE_BLOCK) / 10.0) * 10.0
            foody = round(random.randrange(0, HEIGHT - SNAKE_BLOCK) / 10.0) * 10.0
            Length_of_snake += 1

        clock.tick(SNAKE_SPEED)

    pygame.quit()
    quit()

gameLoop()
\`\`\`

You can save this code in a file named \`snake.py\` and run it using your terminal:
\`python snake.py\`

Let me know if you want to add features like obstacles or high scores!`,
        timestamp: NOW - HOUR * 2
      }
    ]
  },
  {
    id: 'mock-session-2',
    title: 'Trip to Tokyo Planning',
    updatedAt: NOW - HOUR * 5,
    messages: [
      {
        id: 'm2-1',
        role: Role.USER,
        content: 'I am planning a 5-day trip to Tokyo in April. What should I visit?',
        timestamp: NOW - HOUR * 5 - 120000
      },
      {
        id: 'm2-2',
        role: Role.MODEL,
        content: `April is a fantastic time to visit Tokyo, primarily due to the **Cherry Blossoms (Sakura)**! Here is a curated 5-day itinerary for you.

### Day 1: Historic Tokyo & Asakusa
*   **Morning**: Visit **Senso-ji Temple** in Asakusa. Walk through Kaminarimon (Thunder Gate).
*   **Lunch**: Try Tempura or Unagi near Nakamise Street.
*   **Afternoon**: Take a river cruise to **Hama-rikyu Gardens**.
*   **Evening**: View Tokyo Skytree for sunset.

### Day 2: Modern Pop Culture & Shopping
*   **Morning**: **Harajuku** (Takeshita Street) for fashion and crepes. Visit **Meiji Jingu Shrine** nearby for peace.
*   **Afternoon**: **Shibuya Crossing**. Visit the Hachiko statue. Explore Shibuya Sky for views.
*   **Evening**: Shinjuku. See the 3D Cat billboard and explore Omoide Yokocho (Piss Alley) for Yakitori.

### Day 3: Art & Bay Area
*   **Morning**: **TeamLab Planets** in Toyosu (book well in advance!).
*   **Afternoon**: Odaiba. See the Unicorn Gundam statue and the Statue of Liberty replica.
*   **Evening**: Dinner in Ginza.

### Day 4: Nature & Culture
*   **Morning**: **Ueno Park**. Great for cherry blossoms. Visit the Tokyo National Museum.
*   **Afternoon**: Akihabara for electronics and anime culture.
*   **Evening**: Roppongi Hills for art galleries and nightlife.

### Day 5: Day Trip (Optional)
*   I recommend a day trip to **Kamakura** (Giant Buddha) or **Hakone** (Hot springs & views of Mt. Fuji).

**Tip**: Get a Suica or Pasmo card for trains, and bring comfortable walking shoes!`,
        timestamp: NOW - HOUR * 5 - 60000
      },
      {
        id: 'm2-3',
        role: Role.USER,
        content: 'What is the weather like?',
        timestamp: NOW - HOUR * 5
      },
      {
        id: 'm2-4',
        role: Role.MODEL,
        content: 'In April, Tokyo weather is generally mild and pleasant, but can be a bit unpredictable.\n\n*   **Temperature**: Average highs of **19°C (66°F)** and lows of **10°C (50°F)**.\n*   **Rain**: There are about 9-10 rainy days in April, so packing a compact umbrella is wise.\n*   **Clothing**: Layers are key. A light jacket or trench coat, cardigans, and long-sleeve shirts are perfect.',
        timestamp: NOW - HOUR * 5
      }
    ]
  },
  {
    id: 'mock-session-3',
    title: 'Explain Quantum Entanglement',
    updatedAt: NOW - HOUR * 24,
    messages: [
      {
        id: 'm3-1',
        role: Role.USER,
        content: 'Explain quantum entanglement like I am 5.',
        timestamp: NOW - HOUR * 24
      },
      {
        id: 'm3-2',
        role: Role.MODEL,
        content: `Imagine you have a pair of magical dice.

Normally, if you roll two dice in different rooms, the numbers they show are totally random. One might be a 6, the other might be a 2.

**Quantum Entanglement** is like connecting these dice in a special way. Once they are "entangled," no matter how far apart they are—even if one is on Earth and the other is on Mars—they will always match.

If you roll one die and it lands on **6**, you instantly know the other die is also a **6** (or a specific matching number), without even looking at it!

Scientists don't fully know *how* the dice send this message instantly (faster than light!), but they know it happens. It's what Einstein called "spooky action at a distance."`,
        timestamp: NOW - HOUR * 24
      }
    ]
  },
  {
    id: 'mock-session-4',
    title: 'React useEffect Debugging',
    updatedAt: NOW - HOUR * 48,
    messages: [
      {
        id: 'm4-1',
        role: Role.USER,
        content: 'My useEffect is running twice. Why?',
        timestamp: NOW - HOUR * 48
      },
      {
        id: 'm4-2',
        role: Role.MODEL,
        content: `This is a very common behavior in React 18+.

If you are running your app in **Strict Mode** (which is default in \`create-react-app\` and Vite), React intentionally mounts, unmounts, and remounts your components during development.

### Why?
React does this to help you find bugs. Specifically, it checks if your effects handle cleanup correctly.

### Example
\`\`\`javascript
useEffect(() => {
  console.log("Component mounted");
  
  return () => {
    console.log("Component unmounted");
  };
}, []);
\`\`\`

**In Development (Strict Mode):**
1.  Log: "Component mounted"
2.  Log: "Component unmounted" (Cleanup runs)
3.  Log: "Component mounted" (Runs again)

**In Production:**
It will only run once.

**Fix:**
You usually don't need to "fix" this, as it's a development-only safety check. Ensure your cleanup function resets whatever the effect started (like clearing intervals or removing event listeners).`,
        timestamp: NOW - HOUR * 48
      }
    ]
  },
  {
    id: 'mock-session-5',
    title: 'Latest AI News Search',
    updatedAt: NOW - HOUR * 0.5,
    messages: [
      {
        id: 'm5-1',
        role: Role.USER,
        content: 'What are the latest updates on GPT-5?',
        timestamp: NOW - HOUR * 0.5
      },
      {
        id: 'm5-2',
        role: Role.MODEL,
        content: `Based on the latest information available in the industry:

1.  **Timeline**: OpenAI has not officially confirmed a release date for GPT-5. Sam Altman has suggested it might be released later in 2025, emphasizing that they "have a lot of work to do."

2.  **Capabilities**: Rumors and early statements suggest:
    *   **Better Reasoning**: Significant improvements in logic and math.
    *   **Multimodality**: Native handling of text, audio, image, and video.
    *   **Personalization**: Deeper memory and understanding of user context.

3.  **Code Name**: Some internal projects like "Project Strawberry" (focused on reasoning) and "Orion" are often discussed in relation to the next generation of models.

*Note: The AI field moves fast, so specific details are subject to change until the official announcement.*`,
        timestamp: NOW - HOUR * 0.5
      }
    ]
  }
];
