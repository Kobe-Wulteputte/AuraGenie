import { createRouter, createWebHistory } from "vue-router";
import ChatView from "@/views/ChatView.vue";
import { registerGuard } from "./guard";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "chat",
      component: ChatView,
      meta: {
        requiresAuth: true,
      },
    },
  ],
});
registerGuard(router);
export default router;
