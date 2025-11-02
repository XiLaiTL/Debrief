#!/bin/bash

# 设置默认项目名称
DEFAULT_PROJECT_NAME="Debrief"

# 设置默认发布地址
DEFAULT_PUBLISH_PATH="E:\Steam\steamapps\common\Escape from Duckov\Duckov_Data\Mods"
# DEFAULT_PUBLISH_PATH="build"

# 从resources/info.ini文件中读取name字段作为项目名称
if [ -f "resources/info.ini" ]; then
    # 使用grep和cut命令提取name字段的值
    PROJECT_NAME=$(grep "name =" resources/info.ini | cut -d'=' -f2 | tr -d '[:space:]')
    
    # 检查是否成功读取了项目名称，如果未读取到则使用默认值
    if [ -z "$PROJECT_NAME" ]; then
        echo "警告：无法从resources/info.ini中读取name字段，使用默认值'$DEFAULT_PROJECT_NAME'"
        PROJECT_NAME="$DEFAULT_PROJECT_NAME"
    fi
else
    echo "警告：未找到resources/info.ini文件，使用默认值'$DEFAULT_PROJECT_NAME'"
    PROJECT_NAME="$DEFAULT_PROJECT_NAME"
fi

# 创建目标目录（如果不存在）
mkdir -p "$DEFAULT_PUBLISH_PATH/${PROJECT_NAME}"

# 复制build文件夹下与项目名相同的dll文件
if [ -f "build/${PROJECT_NAME}.dll" ]; then
    cp -f "build/${PROJECT_NAME}.dll" "$DEFAULT_PUBLISH_PATH/${PROJECT_NAME}/${PROJECT_NAME}.dll"
    echo "已复制 ${PROJECT_NAME}.dll 到 $DEFAULT_PUBLISH_PATH/${PROJECT_NAME}/"
else
    echo "警告：未找到 build/${PROJECT_NAME}.dll 文件"
fi

# 复制resources目录下的所有文件
if [ -d "resources" ]; then
    cp -rf "resources/"* "$DEFAULT_PUBLISH_PATH/${PROJECT_NAME}/"
    echo "已复制resources目录下的所有文件到 $DEFAULT_PUBLISH_PATH/${PROJECT_NAME}/"
else
    echo "警告：未找到resources目录"
fi

# 脚本结束后暂停，防止窗口自动关闭
read -p "按回车键继续..." pause
