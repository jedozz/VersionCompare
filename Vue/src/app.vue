<template>
  <v-app class="root">
    <v-navigation-drawer v-model="drawer" app clipped class="left">
      <v-list :two-line="true">
        <v-subheader class="customers-title">
          <span>项目</span>
        </v-subheader>
        <v-list-item-group color="primary" v-model="project">
          <v-list-item v-for="(item, i) in projects" :key="i" :value="item">
            <v-list-item-content>
              <v-list-item-title v-html="item.Name"></v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list-item-group>
      </v-list>
    </v-navigation-drawer>
    <v-app-bar class="top" app clipped-left>
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
      <v-toolbar-title>版本比较</v-toolbar-title>
    </v-app-bar>
    <v-main app>
      <div class="main-container">
        <v-card v-if="project != null" :elevation="8">
          <v-card-text>
            <v-container>
              <v-row>
                <v-btn class="mr-3" color="primary" small @click="downloadMore"
                  >从代码库下载</v-btn
                >
                <v-btn class="mr-3" color="primary" small @click="uploadMore"
                  >上传到代码库</v-btn
                >
                <v-checkbox
                  v-model="showAll"
                  hide-details
                  class="shrink mr-auto mt-0"
                  label="显示项目中没有的代码文件"
                ></v-checkbox>
                <v-btn
                  color="primary mr-3"
                  small
                  @click="openDirOfProject(project)"
                  >打开项目文件夹</v-btn
                >
                <v-btn color="primary" small @click="refresh()">刷新</v-btn>
              </v-row>
            </v-container>
          </v-card-text>
          <v-expansion-panels>
            <v-expansion-panel
              v-for="(cr, i) in project.CodeRelations"
              :key="i"
              v-show="cr.CompareResult != 5 || showAll"
            >
              <v-expansion-panel-header>
                <div>
                  <span>{{
                    cr.CodeInProject == null
                      ? cr.CodeInStore.Path
                      : cr.CodeInProject.Path
                  }}</span>
                  <span class="code-filename">{{
                    cr.CodeInProject == null
                      ? cr.CodeInStore.FileName
                      : cr.CodeInProject.FileName
                  }}</span>
                  <i
                    v-if="cr.CompareResult == 0"
                    class="code-icon code-icon-ok fa fa-check-circle"
                    aria-hidden="true"
                  ></i>
                  <i
                    v-else-if="cr.CompareResult == 1"
                    class="code-icon code-icon-warning fa fa-download"
                    aria-hidden="true"
                  ></i>
                  <i
                    v-else-if="cr.CompareResult == 2"
                    class="code-icon code-icon-warning fa fa-upload"
                    aria-hidden="true"
                  ></i>
                  <i
                    v-else-if="cr.CompareResult == 5"
                    class="code-icon fa fa-info-circle"
                  ></i>
                  <i
                    v-else
                    class="code-icon code-icon-error fa fa-exclamation-triangle"
                  ></i>
                  <span
                    v-if="cr.CompareResult != 0"
                    class="code-compareresultdes"
                    >{{ cr.CompareResultDes }}</span
                  >
                </div>
              </v-expansion-panel-header>
              <v-expansion-panel-content>
                <v-list>
                  <v-list-item>
                    <v-row>
                      <v-col>对比结果: {{ cr.CompareResultDes }}</v-col>
                      <v-col v-if="cr.CodeInProject != null">
                        项目中的版本号:
                        {{ cr.CodeInProject.Version }}</v-col
                      >
                      <v-col
                        >代码库中的版本号: {{ cr.CodeInStore.Version }}</v-col
                      >
                    </v-row>
                  </v-list-item>
                  <v-list-item>
                    <span class="mr-3"
                      >在代码库的位置: {{ cr.CodeInStore.Path }}</span
                    >
                    <v-btn
                      v-if="cr.CompareResult != 5"
                      @click="compareContent(cr)"
                      color="primary"
                      small
                      class="mr-3"
                      >对比文件</v-btn
                    >
                    <v-btn
                      v-if="cr.CompareResult != 5"
                      @click="openDirOfFile(cr.CodeInProject.Path)"
                      color="primary"
                      small
                      class="mr-3"
                      >文件在项目中的位置</v-btn
                    >
                    <v-btn
                      @click="openDirOfFile(cr.CodeInStore.Path)"
                      color="primary"
                      small
                      class="mr-3"
                      >文件在代码库的位置</v-btn
                    >
                    <v-btn
                      v-if="
                        cr.CompareResult == 1 ||
                        cr.CompareResult == 3 ||
                        cr.CompareResult == 4
                      "
                      @click="download(cr)"
                      color="primary"
                      small
                      class="mr-3"
                      >从代码库下载</v-btn
                    >
                    <v-btn
                      v-if="cr.CompareResult == 2 || cr.CompareResult == 3"
                      @click="upload(cr)"
                      color="primary"
                      small
                      class="mr-3"
                      >上传到代码库</v-btn
                    >
                  </v-list-item>
                </v-list>
              </v-expansion-panel-content>
            </v-expansion-panel>
          </v-expansion-panels>
        </v-card>
      </div>
    </v-main>
    <v-footer app></v-footer>
    <v-alert
      :type="alertType"
      :value="alertShow"
      class="alert-position"
      transition="scale-transition"
      dismissible
      >{{ alertMsg }}</v-alert
    >
    <v-overlay :value="loading">
      <v-progress-circular
        :size="70"
        :width="7"
        indeterminate
        color="primary"
      ></v-progress-circular>
    </v-overlay>
    <v-dialog v-model="showDialog" width="500">
      <v-card>
        <v-card-text>
          <i
            class="fa fa-question-circle dialog-icon"
            :class="[dialogType]"
            aria-hidden="true"
          ></i>
          <span class="dialog-text">{{ dialogMsg }}</span>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn
            class="dialog-btn"
            color="primary"
            @click="dialogCommand('cancel')"
            >取消</v-btn
          >
          <v-btn class="dialog-btn" color="primary" @click="dialogCommand('ok')"
            >确定</v-btn
          >
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>
<style>
@import "~@assets/css/style.css";

.root .top .v-toolbar__title {
  font-size: 2rem;
}

.root .left .customers-title {
  font-size: 1.6rem;
  font-weight: bolder;
  position: relative;
}

.root .left .v-list-item__title {
  font-size: 1.3rem;
}

.root .left .v-list-item__subtitle {
  font-size: 1.1rem;
}

.root .container {
  padding: 20px;
}

.root .card-data {
  position: absolute;
  left: 20px;
  top: 240px;
  right: 20px;
  bottom: 0px;
  min-height: 300px;
  margin-bottom: 20px;
  overflow-y: auto;
}

.root .main-container {
  padding: 20px;
}

.root .main-container .code-filename {
  margin-left: 15px;
}

.root .main-container .code-icon {
  margin-left: 15px;
  font-size: 14px;
}

.root .main-container .code-icon-ok {
  color: #00a709;
}

.root .main-container .code-icon-warning {
  color: #f1ae69;
}

.root .main-container .code-icon-error {
  color: #ff3c35;
}

.root .main-container .code-compareresultdes {
  margin-left: 5px;
}

.root .alert-position {
  position: fixed;
  top: 50px;
  z-index: 10;
  left: 50%;
  transform: translateX(-50%);
}

.root .dialog-icon {
  font-size: 80px;
  margin-top: 20px;
}

.root .dialog-text {
  margin-left: 30px;
  font-size: 20px;
}

.root .dialog-icon.confirm {
  color: #f1ae69;
}
</style>
<script>
import comm from "@assets/js/comm";
export default {
  data() {
    return {
      showAll: false,
      drawer: null,
      alertShow: false,
      alertMsg: "",
      alertType: "error",
      alertAutoCloseSeed: null,
      projects: [],
      project: null,
      loading: false,
      showDialog: false,
      dialogMsg: "",
      dialogType: "",
      dialogCommand: null,
    };
  },
  components: {},
  watch: {
    project(newV) {
      console.log(newV);
    },
  },
  mounted() {
    this.compareAll();
  },
  methods: {
    download(codeRelation) {
      comm
        .post("Download", codeRelation)
        .then(() => {
          this.refresh();
        })
        .catch((err) => {
          this.showError(err.message);
        });
    },
    upload(codeRelation) {
      this.comfirm("确认上传?").then((value) => {
        if (value == "ok") {
          comm
            .post("Upload", codeRelation)
            .then(() => {
              this.refresh();
            })
            .catch((err) => {
              this.showError(err.message);
            });
        }
      });
    },
    downloadMore() {
      var index = this.projects.indexOf(this.project);
      comm
        .post("DownloadMore", index)
        .then(() => {
          this.refresh();
        })
        .catch((err) => {
          this.showError(err.message);
        });
    },
    uploadMore() {
      this.comfirm("确认上传?").then((value) => {
        if (value == "ok") {
          var index = this.projects.indexOf(this.project);
          comm
            .post("UploadMore", index)
            .then(() => {
              this.refresh();
            })
            .catch((err) => {
              this.showError(err.message);
            });
        }
      });
    },
    refresh() {
      this.loading = true;
      var index = this.projects.indexOf(this.project);
      comm
        .post("Compare", index)
        .then((r) => {
          this.projects[index] = r;
          this.project = r;
        })
        .catch((err) => {
          this.showError(err.message);
        })
        .finally(() => {
          this.loading = false;
        });
    },
    compareContent(code) {
      comm
        .post("StartBeyondCompare", code)
        .then(() => {
          this.showInfo("正在打开BeyondCompare");
        })
        .catch((err) => {
          this.showError(err.message);
        });
    },
    openDirOfProject(project) {
      var index = this.projects.indexOf(project);
      comm
        .post("OpenProjectDir", index)
        .then(() => {
          this.showInfo("正在开文件夹");
        })
        .catch((err) => {
          this.showError(err.message);
        });
    },
    openDirOfFile(filePath) {
      comm
        .post("OpenDirOfFile", filePath)
        .then(() => {
          this.showInfo("正在开文件夹");
        })
        .catch((err) => {
          this.showError(err.message);
        });
    },
    compareAll() {
      this.loading = true;
      comm
        .get("CompareAll")
        .then((r) => {
          this.projects = r;
          this.loading = false;
          if (this.projects.length > 0) {
            this.project = this.projects[0];
          }
        })
        .catch((err) => {
          this.showError(err.message);
        })
        .finally(() => {
          this.loading = false;
        });
    },
    showAlert(msg, type, timeout) {
      var that = this;
      clearTimeout(that.alertAutoCloseSeed);
      that.alertShow = true;
      that.alertMsg = msg;
      that.alertType = type;
      that.alertAutoCloseSeed = setTimeout(() => {
        that.alertShow = false;
      }, timeout);
    },
    showError(msg) {
      this.showAlert(msg, "error", 10000);
    },
    showInfo(msg) {
      this.showAlert(msg, "info", 2000);
    },
    comfirm(msg) {
      return new Promise((resolve) => {
        this.dialogMsg = msg;
        this.showDialog = true;
        this.dialogType = "confirm";
        this.dialogCommand = (value) => {
          this.dialogMsg = "";
          this.showDialog = false;
          resolve(value);
        };
      });
    },
  },
};
</script>
