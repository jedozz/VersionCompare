import Vue from 'vue'
import App from './app.vue'
import lang from '@assets/js/language.js'
import '@assets/css/style.css'
import vuetify from './plugins/vuetify.js'
import '@assets/js/yj.extend.js'
import '@fortawesome/fontawesome-free/css/all.css'

Vue.mixin({
    data() {
        return {
            lang: lang
        }
    }
})

new Vue({
    vuetify,
    render: h => h(App)
}).$mount('#app')