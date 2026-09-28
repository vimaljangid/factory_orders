# 🏭 Factory Orders & Priority Dispatch (Live Sync)

A real-time manufacturing order management and priority tagging system built with **C# (.NET 8 Blazor Server & SignalR)**. 

Designed for factory floors where **anyone can place manufacturing orders**, **anyone can track status**, and users can tag **urgent items** and **#1 First to Produce** priorities that update instantly on all connected devices without refreshing.

---

## 🚀 Features

- **⚡ Real-Time Live Sync (SignalR):** When any person places an order, changes priority, or claims a job, the change reflects on all connected phones in milliseconds.
- **🚨 Urgent & First-To-Produce Tagging:**
  - One-tap `⚡ FIRST TO READY` tagging that pins orders to the very top of the production queue.
  - Urgent priority alerts with red styling and audio chime notification.
- **📱 Zero Android Studio Required:** Runs as a responsive **Progressive Web App (PWA)** that can be installed directly to Android home screens via Chrome with a real app icon and full-screen view.
- **🛠 Production Workflow Tracking:**
  - `Waiting to Make` (Pending)
  - `In Production` (Shows who claimed the job, e.g. "Worker: Rajesh")
  - `Ready / Done` (Completed with timestamp)
- **💾 Local Persistence:** Built-in SQLite database (`factory_orders.db`) so orders are saved across restarts.
- **☁️ Free Cloud Ready:** Includes a `Dockerfile` ready for 1-click free hosting on Render, Railway, or Fly.io.

---

## 🖥️ 1. How to Run Locally and Preview on PC

You can test the app right now on your computer without installing Android Studio:

1. Double-click **`run_local.bat`** in this folder (or run `dotnet run --urls "http://0.0.0.0:5000"` from terminal).
2. Open your browser and go to:
   ```
   http://localhost:5000
   ```
3. **To see the exact Android phone view:**
   - Press **F12** on your keyboard (Inspect Element in Chrome/Edge).
   - Click the **Device Toolbar icon** (or press `Ctrl + Shift + M`).
   - Select a mobile screen size (e.g. *Pixel 7* or *Samsung Galaxy*).
   - You can open a second browser window side-by-side to see orders sync live between them in real-time!

---

## 📱 2. How to Use on Android Phones via Local Wi-Fi (No Cloud Needed)

If your computer and your phone are on the same factory/home Wi-Fi:

1. Find your computer's local IP address:
   - Open Command Prompt and type `ipconfig`.
   - Look for **IPv4 Address** (e.g. `192.168.1.15`).
2. Run `run_local.bat` on your PC.
3. On your Android phone, open Chrome and type:
   ```
   http://192.168.1.15:5000
   ```
   *(replace `192.168.1.15` with your actual IPv4 address)*
4. **Install as App:**
   - In Chrome, tap the 3 dots menu (⋮) $\rightarrow$ Tap **"Add to Home screen"** or **"Install app"**.
   - The app will now appear on your phone's home screen with an app icon. Opening it launches a clean, full-screen mobile app!

---

## ☁️ 3. How to Deploy on Free Cloud Hosting (Access Anywhere over Internet)

To let anyone in your factory or off-site access the app from anywhere, you can host it for free using **Render.com**:

### Option A: Render.com (100% Free)
1. Push this folder to a GitHub repository (e.g. `github.com/yourname/factory_orders`).
2. Go to [Render.com](https://render.com) and sign up for a free account.
3. Click **"New +"** $\rightarrow$ **"Web Service"**.
4. Connect your GitHub repository.
5. In the settings:
   - **Environment:** `Docker` (it will automatically detect the provided `Dockerfile`).
   - **Instance Type:** `Free`.
6. Click **"Create Web Service"**.
7. Render will build and launch your app with a free HTTPS link (e.g. `https://factory-orders.onrender.com`).
8. Anyone can open this link on their Android phone and tap **"Install / Add to Home screen"**!

### Option B: Railway.app
1. Go to [Railway.app](https://railway.app).
2. Click **New Project** $\rightarrow$ **Deploy from GitHub repo**.
3. Select your repo. Railway will automatically build and deploy using the `Dockerfile`.
