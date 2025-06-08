// Clean, multi-use version for both single-slide and multi-slide editors
// Only export window.startDrawingInContainer, do not declare username globally, do not use Swal or document.ready

window.startDrawingInContainer = function(container, slideId, username) {
    console.log('Initializing editor', container, slideId, username);
    var editor;
    var connectionLoadCanvas;

    function startDrawing() {
        const settings = {
            wheelEventsEnabled: 'only-if-focused',
        };
        editor = new jsdraw.Editor(container, settings);
        const toolbar = editor.addToolbar();

        toolbar.addExitButton(() => {
            editor.remove();
        });

        toolbar.addActionButton('|Sync|', async () => {
            try {
                await saveNewSvg();
                editor.remove();
                startDrawing();
            } catch (error) {
                console.error('Error during sync:', error);
                toastr.error('Failed to sync slide');
            }
        });

        toolbar.addActionButton('|Download|', () => {
            var jpgDataUrl = editor.toDataURL();
            download(jpgDataUrl, `drawing-${slideId}.jpg`);
        });

        editor.getRootElement().style.height = '95vh';
        editor.getRootElement().style.border = '2px solid gray';

        const addToHistory = false;
        editor.dispatch(editor.setBackgroundStyle({
            autoresize: true,
        }), addToHistory);

        getExistingSvg();

        editor.notifier.on(jsdraw.EditorEventType.CommandDone, (evt) => {
            if (evt.kind !== jsdraw.EditorEventType.CommandDone) {
                throw new Error('Incorrect event type');
            }

            if (evt.command instanceof jsdraw.SerializableCommand) {
                postToServer(JSON.stringify({
                    command: evt.command.serialize()
                }));
            } else {
                console.log('!', evt.command, 'instanceof jsdraw.SerializableCommand');
            }
        });

        editor.notifier.on(jsdraw.EditorEventType.CommandUndone, (evt) => {
            if (evt.kind !== jsdraw.EditorEventType.CommandUndone) {
                return;
            }

            if (!(evt.command instanceof jsdraw.SerializableCommand)) {
                console.log('Not serializable!', evt.command);
                return;
            }

            postToServer(JSON.stringify({
                command: jsdraw.invertCommand(evt.command).serialize()
            }));
        });
    }

    function startCommentConnection() {
        connectionLoadCanvas = new signalR.HubConnectionBuilder()
            .withUrl("/presentationHub", signalR.HttpTransportType.WebSockets)
            .withAutomaticReconnect()
            .build();

        connectionLoadCanvas.on("UpdateDrawing", (drawingData) => {
            processUpdates(drawingData);
        });

        connectionLoadCanvas.on("SvgSaved", (savedSlideId) => {
            if (savedSlideId === slideId.toString()) {
                toastr.success('Slide saved successfully!');
            }
        });

        connectionLoadCanvas.on("Error", (errorMessage) => {
            toastr.error(errorMessage);
        });

        connectionLoadCanvas.start()
            .then(() => {
                console.log("SignalR Connected");
                return connectionLoadCanvas.invoke("JoinBoard", slideId.toString(), username);
            })
            .then((msg) => {
                console.log(msg);
            })
            .catch(err => {
                console.error("SignalR Connection Error:", err);
                toastr.error("Failed to connect to server");
            });
    }

    function getExistingSvg() {
        $.ajax({
            type: 'GET',
            url: `/get-svg/${slideId}`,
            dataType: 'text',
            success: function (svgText) {
                if (svgText && svgText.length > 0) {
                    editor.loadFromSVG(svgText);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error retrieving SVG data:', error);
            }
        });
    }

    const postToServer = async (commandData) => {
        try {
            await connectionLoadCanvas.invoke("Modify", slideId.toString(), commandData);
        } catch (e) {
            console.error('Error posting command', e);
        }
    };

    const processUpdates = async (drawingData) => {
        try {
            const json = JSON.parse(drawingData);
            try {
                const command = jsdraw.SerializableCommand.deserialize(json.command, editor);
                await command.apply(editor);
            } catch (e) {
                console.warn('Error parsing command', e);
            }
        } catch (e) {
            console.error('Error fetching updates', e);
        }
    };

    async function saveNewSvg() {
        try {
            var svgData = await editor.toSVGAsync();
            var svgString = svgData.outerHTML;
            
            // Save using SignalR
            await connectionLoadCanvas.invoke("SaveSvg", slideId.toString(), svgString);
            
            // Update the thumbnail
            const slideThumbnail = document.querySelector(`[data-slide-id="${slideId}"]`);
            if (slideThumbnail) {
                const img = document.createElement('img');
                img.src = `data:image/svg+xml;base64,${btoa(svgString)}`;
                img.alt = `Slide ${slideThumbnail.getAttribute('data-slide-number')}`;
                img.style.width = '100%';
                img.style.height = '100%';
                img.style.objectFit = 'contain';
                
                // Clear the loading text and add the image
                slideThumbnail.innerHTML = '';
                slideThumbnail.appendChild(img);
            }
            
            toastr.success('Slide saved successfully!');
        } catch (error) {
            console.error('Error saving SVG:', error);
            toastr.error('Failed to save slide');
            throw error; // Re-throw to handle in the calling function
        }
    }

    function download(dataurl, filename) {
        const link = document.createElement("a");
        link.href = dataurl;
        link.download = filename;
        link.click();
    }

    // Start everything
    startDrawing();
    startCommentConnection();
}