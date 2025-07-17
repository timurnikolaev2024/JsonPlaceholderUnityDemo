using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonPlaceholderSdk;
using JsonPlaceholderSdk.Models;
using UnityEngine;
using UnityEngine.UI;

public class JsonPlaceholderManager : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    public GameObject itemViewPrefab;
    public Transform contentParent;

    [Header("Input Fields")]
    public InputField titleInputField;
    public InputField bodyInputField;
    public InputField postIdInputField;

    [Header("Buttons")]
    public Button addButton;
    public Button updateButton;
    public Button deleteButton;
    public Button refreshButton;

    private JsonPlaceholderClient client;
    private List<Post> posts = new();

    private void Awake()
    {
        client = new JsonPlaceholderClient(
            log: msg => Debug.Log(msg)
        );

        addButton.onClick.AddListener(() => _ = AddPost());
        updateButton.onClick.AddListener(() => _ = UpdatePost());
        deleteButton.onClick.AddListener(() => _ = DeletePost());
        refreshButton.onClick.AddListener(() => _ = LoadPostsFromServer());
    }

    private async void Start()
    {
        await LoadPostsFromServer();
    }

    private async Task LoadPostsFromServer()
    {
        posts = await client.GetPostsAsync();
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var post in posts)
        {
            var go = Instantiate(itemViewPrefab, contentParent);
            var view = go.GetComponent<ItemView>();
            view.Setup(post.Id.ToString(), post.Title, post.Body);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
    }

    private async Task AddPost()
    {
        var newPost = new Post
        {
            UserId = 1,
            Title  = titleInputField.text,
            Body   = bodyInputField.text
        };

        var created = await client.CreatePostAsync(newPost);

        posts.Add(created);
        RefreshUI();

        titleInputField.text = "";
        bodyInputField.text  = "";
    }

    private async Task UpdatePost()
    {
        if (!int.TryParse(postIdInputField.text, out var id))
        {
            Debug.LogWarning("Please enter a valid ID to update.");
            return;
        }

        var existing = posts.FirstOrDefault(p => p.Id == id);
        if (existing == null)
        {
            Debug.LogWarning($"Post with ID={id} not found in the local list.");
            return;
        }

        var updated = new Post
        {
            Id     = id,
            UserId = existing.UserId,
            Title  = titleInputField.text,
            Body   = bodyInputField.text
        };

        var result = await client.UpdatePostAsync(updated);

        var index = posts.FindIndex(p => p.Id == id);
        posts[index] = result;
        RefreshUI();

        postIdInputField.text = "";
        titleInputField.text  = "";
        bodyInputField.text   = "";
    }

    private async Task DeletePost()
    {
        if (!int.TryParse(postIdInputField.text, out var id))
        {
            Debug.LogWarning("Please enter a valid ID to delete.");
            return;
        }

        bool success = await client.DeletePostAsync(id);
        if (success)
        {
            posts.RemoveAll(p => p.Id == id);
            RefreshUI();
        }
        else
        {
            Debug.LogWarning($"Server returned an error while deleting ID={id}.");
        }

        postIdInputField.text = "";
    }
}
