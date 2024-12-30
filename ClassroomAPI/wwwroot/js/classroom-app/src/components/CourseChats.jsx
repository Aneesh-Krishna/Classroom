import { useEffect, useState, useRef } from "react";
import styled from "styled-components";
import { NavLink } from "react-router-dom";
import '../styles/CourseChats.css';

function CourseChats({ authToken, adminId, courseId, courseName }) {
    const [chats, setChats] = useState(null);
    const [error, setError] = useState(null);
    const [file, setFile] = useState(null);
    const [chatMessage, setChatMessage] = useState('');
    const [refreshTrigger, setRefreshTrigger] = useState(0);
    const chatBodyRef = useRef(null);  // Ref to the chat body

    const formatSentTime = (sentTime) => {
        const date = new Date(sentTime);
    
        // Extract date components
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based
        const year = date.getFullYear();
    
        // Extract time components
        let hours = date.getHours();
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const ampm = hours >= 12 ? 'PM' : 'AM';
    
        // Convert to 12-hour format
        hours = hours % 12;
        hours = hours ? hours : 12; // The hour '0' should be '12'
    
        const formattedDate = `${day}-${month}-${year}`;
        const formattedTime = `${hours}:${minutes} ${ampm}`;
    
        return `${formattedDate} ${formattedTime}`;
    };

    const handleFileChange = (e) => {
        const selectedFile = e.target.files[0];
        setFile(selectedFile);
    };

    const fetchAllChats = async () => {
        try {
            const response = await fetch(`https://localhost:7110/api/chat/${courseId}/GetAllChats`, {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                },
            });

            if (response.ok) {
                const data = await response.json();
                setChats(data?.$values || []); // Handle the array correctly
            } else {
                throw new Error(`Error ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            setError(error.message);
            console.error("Error fetching chats:", error);
        }
    };

    const handleSendMessage = async (e) => {
        e.preventDefault();

        try {
            const form = new FormData();
            form.append(`message`, chatMessage);
            if (file) {
                form.append(`file`, file);
                if (chatMessage === "") {
                    setChatMessage("Sent a file");
                }
            }

            const response = await fetch(`https://localhost:7110/api/chat/${courseId}/SendMessage`, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                },
                body: form,
            });

            if (response.ok) {
                setChatMessage('');
                setFile(null);
                setRefreshTrigger((prev) => prev + 1);
            } else {
                console.error("An error occurred while sending the message...", response.text);
            }
        } catch (error) {
            console.error("Something went wrong...", error);
        }
    };

    const handleDownloadFile = async (fileName) => {
        try {
            const response = await fetch(`https://localhost:7110/api/file/${fileName}`, {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                },
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement("a");
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            } else {
                console.error("An error occurred while downloading...", response.statusText);
            }
        } catch (error) {
            console.error("Something went wrong...:", error);
        }
    };

    useEffect(() => {
        fetchAllChats();
    }, [authToken, courseId, refreshTrigger]);

    useEffect(() => {
        // Scroll to the bottom of the chat body whenever new chats are added or sent
        if (chatBodyRef.current) {
            chatBodyRef.current.scrollTop = chatBodyRef.current.scrollHeight;
        }
    }, [chats, refreshTrigger]); // Runs when chats or refreshTrigger change

    return (
        <StyledWrapper>
            <link href="https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css" rel="stylesheet"/>
            <NavLink to="/courses" className="btn btn-outline-primary me-2 bg-blue-500 text-white font-medium rounded-lg px-4 py-2 hover:bg-blue-600">◄ Back</NavLink>
            <div className="chat-card max-w-lg mx-auto bg-white shadow-lg rounded-lg overflow-hidden" style={{ width: '90%' }}>
                <div className="chat-header bg-blue-500 text-white p-4">
                    <div className="h2 font-bold">{courseName}</div>
                </div>
                <div className="chat-body p-4" ref={chatBodyRef}>
                    <div className="message incoming bg-gray-100 p-3 rounded-lg mb-2">
                        <p>Hello, how can I assist you today?</p>
                    </div>
                    {error ? (
                        <p className="error text-red-500">Failed to load chats: {error}</p>
                    ) : chats === null ? (
                        <div className="message incoming bg-gray-100 p-3 rounded-lg mb-2">
                            <p>Loading chats...</p>
                        </div>
                    ) : chats.length > 0 ? (
                        <ul>
                            {chats.map((chat) => (
                                <li key={chat.chatId} className="chatItem" title={chat.sentAt ? formatSentTime(chat.sentAt) : "Sent time not available"}>
                                    <div className="chatHeader text-sm text-gray-600">
                                        <i>Message from:</i> {chat.senderName || "Unknown"}
                                    </div>
                                    <div className="message incoming bg-gray-100 p-3 rounded-lg mb-2">
                                        <p>{chat.message || "No message content"}</p>
                                        {chat.fileName && (
                                            <div className="chatFile text-blue-500 mt-1">
                                                <strong>File:</strong> <button onClick={() => handleDownloadFile(chat.fileName)} className="text-blue-600 hover:underline">{chat.fileName}</button>
                                            </div>
                                        )}
                                    </div>
                                </li>
                            ))}
                        </ul>
                    ) : (
                        <div className="message incoming bg-gray-100 p-3 rounded-lg mb-2">
                            <p>No chats yet. Start a conversation!</p>
                        </div>
                    )}
                </div>
                <div className="chat-footer flex items-center p-4 bg-gray-50 border-t">
                    <form onSubmit={handleSendMessage} className="flex w-full items-center">
                        <label htmlFor="file-upload" title="Upload a file" className="file-icon cursor-pointer text-blue-500 mr-4">
                            {file ? (
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="2">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                                </svg>
                            ) : (
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="2">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
                                </svg>
                            )}
                        </label>
                        <input
                            id="file-upload"
                            type="file"
                            className="hidden"
                            onChange={handleFileChange}
                            title="Upload a file"
                        />
                        <input
                            type="text"
                            className="message-control flex-1 border border-gray-300 rounded-lg p-2 mr-2"
                            placeholder="Enter your message"
                            onChange={(e) => setChatMessage(e.target.value)}
                            value={chatMessage}
                        />
                        <button type="submit" className="bg-blue-500 text-white font-medium rounded-lg px-4 py-2 hover:bg-blue-600">
                            Send
                        </button>
                    </form>
                </div>
            </div>
        </StyledWrapper>
    );
}

export default CourseChats;

const StyledWrapper = styled.div`
    padding: 16px;

    .chat-header {
        padding: 10px;
        background-color:rgb(213, 213, 213);
        display: flex;
        align-items: center;
    }

    .chat-header .h2 {
        font-size: 16px;
        color: #333;
    }
    h2 {
        margin-bottom: 16px;
    }
    ul {
        list-style: none;
        padding: 0;
    }
    .chatItem {
        border: 1px solid #ccc;
        border-radius: 8px;
        padding: 8px;
        margin-bottom: 8px;
    }
    .chatBody {
        font-size: 14px;
    }
    .chatFile {
        font-style: italic;
        margin-top: 4px;
    }
    .error {
        color: red;
        font-weight: bold;
    }
    .chat-body {
        max-height: 400px;
        overflow-y: auto;
    }
`;
