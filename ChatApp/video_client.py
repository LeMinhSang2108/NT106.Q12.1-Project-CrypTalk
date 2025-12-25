import socket
import threading
import json
import cv2
import tkinter as tk
from tkinter import ttk, messagebox
from PIL import Image, ImageTk
import time
import numpy as np
import sys


SERVER_HOST = '127.0.0.1'
TCP_PORT = 8000
UDP_PORT = 8001

FRAME_WIDTH = 480
FRAME_HEIGHT = 360
JPEG_QUALITY = 40
FPS_LIMIT = 20


class VideoClient:
    
    def __init__(self, root, initial_cam=0):
        self.root = root
        self.root.title("Cryptalk Video Call")
        self.root.geometry("1000x550")
        self.root.configure(bg='#2c3e50')

        self.tcp_socket = None
        self.udp_socket = None
        self.username = None

        self.in_call = False
        self.current_call_id = None

        self.camera = None
        self.camera_running = False
        
        self.cam_index = initial_cam
        
        self.setup_gui()

    
    def setup_gui(self):
        self.status_label = tk.Label(
            self.root, text="Initializing...",
            bg='#34495e', fg='white',
            font=('Segoe UI', 12, 'bold'), pady=10
        )
        self.status_label.pack(fill=tk.X)

        container = tk.Frame(self.root, bg='#2c3e50')
        container.pack(expand=True, fill=tk.BOTH, pady=20)

        self.local_frame = tk.LabelFrame(container, text="Your Camera",
                                         bg='#2c3e50', fg='white')
        self.local_frame.pack(side=tk.LEFT, padx=20)

        self.my_video_label = tk.Label(self.local_frame, bg='black',
                                       width=FRAME_WIDTH, height=FRAME_HEIGHT)
        self.my_video_label.pack(padx=5, pady=5)

        self.remote_frame = tk.LabelFrame(container, text="Partner",
                                          bg='#2c3e50', fg='white')
        self.remote_frame.pack(side=tk.RIGHT, padx=20)

        self.remote_video_label = tk.Label(self.remote_frame, bg='black',
                                           width=FRAME_WIDTH, height=FRAME_HEIGHT)
        self.remote_video_label.pack(padx=5, pady=5)

        self.btn_frame = tk.Frame(self.root, bg='#2c3e50')
        self.btn_frame.pack(pady=15)

        self.exit_btn = tk.Button(
            self.btn_frame, text="END CALL",
            bg='#e74c3c', fg='white',
            font=('Arial', 10, 'bold'),
            padx=20, pady=5, relief=tk.FLAT,
            command=self.on_closing
        )
        self.exit_btn.pack()

        self.username_entry = ttk.Entry(self.root)  # ẩn

    
    def login(self):
        global SERVER_HOST
        self.username = self.username_entry.get().strip()
        if not self.username:
            return

        try:
            if SERVER_HOST.startswith("::ffff:"):
                SERVER_HOST = SERVER_HOST.replace("::ffff:", "")

            # TCP
            self.tcp_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.tcp_socket.settimeout(3)
            self.tcp_socket.connect((SERVER_HOST, TCP_PORT))
            self.tcp_socket.settimeout(None)

            # UDP
            self.udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            self.udp_socket.bind(('0.0.0.0', 0))
            udp_port = self.udp_socket.getsockname()[1]

            self.send_tcp({
                'action': 'login',
                'username': self.username,
                'udp_port': udp_port
            })

            threading.Thread(target=self.listen_tcp, daemon=True).start()
            threading.Thread(target=self.listen_udp, daemon=True).start()

            self.start_camera()
            self.status_label.config(text=f"Logged in as {self.username} | Cam: {self.cam_index}")

        except Exception as e:
            messagebox.showerror("Connection Error", str(e))
            self.root.destroy()

    def start_camera(self):
        
        indices = [self.cam_index] + [i for i in range(6) if i != self.cam_index]
        
        for idx in indices:
            
            cap = cv2.VideoCapture(idx, cv2.CAP_DSHOW)
            if not cap.isOpened():
                cap.release()
                continue

            cap.set(cv2.CAP_PROP_FRAME_WIDTH, FRAME_WIDTH)
            cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)

            # Warm up
            time.sleep(0.5) 

            ok = False
            for _ in range(10):
                ret, frame = cap.read()
                
                if ret and frame is not None and frame.size > 1000:
                    ok = True
                    break
                time.sleep(0.1)

            if ok:
                print(f"[CAMERA] Using camera index {idx}")
                self.camera = cap
                self.camera_running = True
                self.cam_index = idx 
                threading.Thread(target=self.camera_loop, daemon=True).start()
                return 

            cap.release()
        
        messagebox.showwarning("Camera", "No functional camera found")

    def camera_loop(self):
        while self.camera_running:
            try:
                ret, frame = self.camera.read()
                if not ret:
                    time.sleep(0.1)
                    continue

                frame = cv2.flip(frame, 1)
                frame = cv2.resize(frame, (FRAME_WIDTH, FRAME_HEIGHT))
                self.display_image(self.my_video_label, frame)

                if self.in_call and self.current_call_id:
                    _, enc = cv2.imencode(
                        '.jpg', frame,
                        [int(cv2.IMWRITE_JPEG_QUALITY), JPEG_QUALITY]
                    )
                    header = f"{self.username}:{self.current_call_id}||".encode()
                    try:
                        self.udp_socket.sendto(header + enc.tobytes(),
                                             (SERVER_HOST, UDP_PORT))
                    except:
                        pass
            except: pass
            time.sleep(1.0 / FPS_LIMIT)

    
    def listen_tcp(self):
        buffer = ""
        while True:
            try:
                data = self.tcp_socket.recv(4096).decode()
                if not data:
                    break
                buffer += data
                while "\n" in buffer:
                    line, buffer = buffer.split("\n", 1)
                    msg = json.loads(line)
                    self.root.after(0, lambda m=msg: self.process_signal(m))
            except:
                break

    def listen_udp(self):
        while True:
            try:
                data, _ = self.udp_socket.recvfrom(65535)
                idx = data.find(b'||')
                if idx != -1 and self.in_call:
                    frame = cv2.imdecode(
                        np.frombuffer(data[idx+2:], np.uint8),
                        cv2.IMREAD_COLOR
                    )
                    if frame is not None:
                        self.display_image(self.remote_video_label, frame)
            except:
                break

    
    def process_signal(self, msg):
        action = msg.get('action')

        if action == 'incoming_call':
            self.status_label.config(text=f"Incoming call from {msg.get('from')}")
            self.current_call_id = msg.get('call_id')
            self.in_call = True
            self.send_tcp({
                'action': 'accept_call',
                'call_id': self.current_call_id,
                'username': self.username
            })

        elif action == 'call_accepted':
            self.current_call_id = msg.get('call_id')
            self.in_call = True
            self.status_label.config(text="Call connected!")

        elif action == 'call_ended':
            self.in_call = False
            self.current_call_id = None
            self.status_label.config(text="Call ended.")
            self.on_closing()

    
    def display_image(self, label, frame):
        try:
            img = ImageTk.PhotoImage(
                Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
            )
            self.root.after(0, lambda: self._update_label(label, img))
        except:
            pass

    def _update_label(self, label, img):
        label.configure(image=img)
        label.imgtk = img

    def send_tcp(self, msg):
        try:
            self.tcp_socket.sendall((json.dumps(msg) + "\n").encode())
        except:
            pass

    
    def on_closing(self):
        self.camera_running = False
        if self.camera:
            self.camera.release()

        if self.in_call and self.current_call_id:
            self.send_tcp({
                'action': 'end_call',
                'call_id': self.current_call_id,
                'username': self.username
            })

        if self.tcp_socket:
            self.tcp_socket.close()

        self.root.destroy()


if __name__ == '__main__':
    root = tk.Tk()
    
    # TRONG THỰC TẾ: Luôn ưu tiên thử Cam 0 trước.
    # Hàm start_camera bên trên đã có vòng lặp thông minh:
    # Nếu Cam 0 hỏng/bận -> Tự động tìm Cam 1, 2, 3...
    app = VideoClient(root, initial_cam=0)
    
    root.protocol("WM_DELETE_WINDOW", app.on_closing)

    if len(sys.argv) > 1:
        app.username_entry.insert(0, sys.argv[1])
        # Nhận IP từ tham số dòng lệnh do C# truyền vào
        if len(sys.argv) > 4:
            SERVER_HOST = sys.argv[4]

        root.after(500, app.login)

        # Tự động gọi nếu là người khởi xướng
        if len(sys.argv) > 2:
            target, is_caller = sys.argv[2], sys.argv[3]
            if is_caller == "1":
                root.after(1500, lambda: app.send_tcp({
                    'action': 'call',
                    'from': app.username,
                    'to': target
                }))

    root.mainloop()