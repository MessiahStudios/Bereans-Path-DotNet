import { createRouter, createWebHistory } from 'vue-router'
import ReaderView from './views/ReaderView.vue'
import BookmarksView from './views/BookmarksView.vue'
import ChurchesView from './views/ChurchesView.vue'
import LogsView from './views/LogsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'reader', component: ReaderView },
    { path: '/bookmarks', name: 'bookmarks', component: BookmarksView },
    { path: '/churches', name: 'churches', component: ChurchesView },
    { path: '/logs', name: 'logs', component: LogsView },
  ],
})

export default router
