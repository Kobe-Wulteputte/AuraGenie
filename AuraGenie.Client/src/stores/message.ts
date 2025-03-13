import * as signalR from "@microsoft/signalr";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import { useAuthStore } from "./auth";
import type { Message } from "vue-advanced-chat";
import {
  ChatsClient,
  type Message as ApiMessage,
} from "@/api-clients/api-client";

export const useMessageStore = defineStore("message", () => {
  const authStore = useAuthStore();
  const chatClient = new ChatsClient();
  const initialized = ref<boolean>(false);
  const messageList = ref<Message[]>([]);
  const hasMoreChats = ref<boolean>(false);
  const loadingMoreNotifications = ref<boolean>(false);
  const genieTyping = ref(false);

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_API_URL + "/api/message/socket", {
      accessTokenFactory: () =>
        authStore.getAccessToken().then((token) => token ?? ""),
    })
    .withAutomaticReconnect([
      100, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000,
    ])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  async function loadChats() {
    let chats = await chatClient.chatsByRoom(1, messageList.value.length, 30);
    hasMoreChats.value = chats.length === 30;
    let newChats = chats.map((chat: ApiMessage) => {
      let newChat = {
        _id: chat.id?.toString(),
        content: chat.messageContent,
        senderId: chat.senderId,
        username: chat.senderId,
        timestamp: new Date(chat.createdOn!).toLocaleTimeString(),
      } as Message;
      if (chat.replyMessage) {
        newChat.replyMessage = {
          _id: chat.replyMessage.id!.toString(),
          content: chat.replyMessage.messageContent,
          senderId: chat.replyMessage.senderId!,
          username: chat.replyMessage.senderId!,
          timestamp: new Date(
            chat.replyMessage?.createdOn!
          ).toLocaleTimeString(),
        };
      }
      return newChat;
    });
    let newList = [...messageList.value, ...newChats];
    // Distinct
    newList = newList.filter(
      (v, i, a) => a.findIndex((t) => t._id === v._id) === i
    );
    // order by timestamp
    messageList.value = newList.sort(
      (a, b) => Number.parseInt(a._id) - Number.parseInt(b._id)
    );
  }

  async function initialize() {
    if (initialized.value) return;
    connection.on("ReceiveMessage", (message: ApiMessage) => {
      messageList.value = messageList.value.filter(
        (msg) => msg.senderId !== "Typing"
      );
      let newMessage = {
        _id: message.id?.toString(),
        content: message.messageContent,
        senderId: message.senderId,
        username: message.senderId,
        timestamp: new Date(message.createdOn!).toLocaleTimeString(),
      } as Message;
      if (message.replyMessage) {
        newMessage.replyMessage = {
          _id: message.replyMessage.id!.toString(),
          content: message.replyMessage.messageContent,
          senderId: message.replyMessage.senderId!,
          username: message.replyMessage.senderId,
          timestamp: new Date(
            message.replyMessage?.createdOn!
          ).toLocaleTimeString(),
        };
      }
      if (!messageList.value) {
        messageList.value = [newMessage];
        return;
      }
      var oldMessages = [...messageList.value];
      messageList.value = [...oldMessages, newMessage];
    });
    connection.on("GenieTyping", (typing: boolean) => {
      genieTyping.value = typing;
      if (typing)
        messageList.value = [
          ...messageList.value,
          {
            _id: Math.random().toString(),
            content: "Genie is aan het denken...",
            senderId: "Typing",
            username: "",
          },
        ];
      else {
        messageList.value = messageList.value.filter(
          (msg) => msg.senderId !== "Typing"
        );
      }
    });

    connection.onclose(async () => {
      console.log("Connection closed. Attempting to restart...");
      await start();
    });
    await start();

    initialized.value = true;
  }

  async function start() {
    try {
      await connection.start();
      console.log("SignalR Connected.");
    } catch (err) {
      console.error("Connection failed, retrying in 5 seconds...", err);
      setTimeout(start, 1000);
    }
  }

  const states = {
    connection,
    initialized,
    genieTyping,
    messageList,
    hasMoreChats,
    loadingMoreNotifications,
  };

  const functions = { initialize, loadChats };

  return { ...states, ...functions };
});
