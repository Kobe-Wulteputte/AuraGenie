<script setup lang="ts">
import { ChatsClient, Message, RoomsClient } from "@/api-clients/api-client";
import { useAuthStore } from "@/stores/auth";
import { useMessageStore } from "@/stores/message";
import { storeToRefs } from "pinia";
import { computed, onMounted, ref, watch } from "vue";
import { register, type Room, type RoomUser } from "vue-advanced-chat";
import { VueAdvancedChat } from "vue-advanced-chat";

register();

const props = defineProps<{}>();

const messageStore = useMessageStore();
const theme = ref("light");
var { account } = storeToRefs(useAuthStore());
const username = computed(() => account.value?.name);

const rooms = ref<Room[]>([]);
const roomsLoaded = ref(false);
const loadingRooms = ref(false);
const roomsClient = new RoomsClient();
const chatsClient = new ChatsClient();
const { messageList: messages, hasMoreChats } = storeToRefs(messageStore);
const messageActions = ref([]);

const currentRoom = ref<Room>();

const setup = async () => {
  loadingRooms.value = true;
  // rooms.value = await chatService.fetchRooms()
  let dbRooms = await roomsClient.getAllRooms();
  let dbUsers = await roomsClient.getUsersInRoom(1);
  let users = dbUsers.map((user) => {
    return {
      _id: user.username,
      username: user.username,
      avatar: "https://cdn-icons-png.flaticon.com/512/1680/1680326.png",
      status: {
        state: "online",
        lastChanged: new Date().getTime().toLocaleString(),
      },
    } as RoomUser;
  });
  rooms.value = dbRooms.map((room) => {
    return {
      roomId: room.id?.toString(),
      roomName: room.name,
      avatar: "https://cdn-icons-png.flaticon.com/512/1680/1680326.png",
      users: users,
      typingUsers: [],
      lastMessage: {} as any,
    } as Room;
  });

  loadingRooms.value = false;
  roomsLoaded.value = true;
};

async function fetchMessages({ room, options = {} }: any) {
  console.log("fetchMessages", options);
  if (options.reset) {
    messages.value = [];
    currentRoom.value = room;
  }

  await messageStore.loadChats();
}

async function sendMessage({
  roomId,
  content,
  files,
  replyMessage,
  usersTag,
}: any) {
  if (!currentRoom.value) {
    return;
  }
  let message = new Message();
  message.messageContent = content;
  message.roomId = Number.parseInt(currentRoom.value?.roomId);
  message.createdOn = new Date().getTime();
  message.senderId = username.value;
  await chatsClient.sendMessage(message);
}

async function tearDown() {
  rooms.value = [];
  roomsLoaded.value = false;
  loadingRooms.value = false;

  messages.value = [];
}

onMounted(async () => {
  await setup();

  watch(
    () => username,
    async (username) => {
      await tearDown();

      await setup();
    }
  );
});
</script>

<template>
  <vue-advanced-chat
    height="calc(100dvh - 20px)"
    :current-user-id="username"
    :load-first-room="true"
    :single-room="true"
    :room-info-enabled="false"
    :theme="(theme as any)"
    :loading-rooms="loadingRooms"
    :rooms-loaded="roomsLoaded"
    :messages-loaded="!hasMoreChats"
    :message-actions="messageActions"
    :user-tags-enabled="true"
    :show-search="false"
    :show-add-room="false"
    :show-files="false"
    :show-new-messages-divider="false"
    :show-audio="false"
    :show-emojis="false"
    :show-footer="true"
    :show-reaction-emojis="false"
    :rooms="rooms"
    :messages="messages"
    @fetch-messages="fetchMessages($event.detail[0])"
    @send-message="sendMessage($event.detail[0])"
  />
</template>
